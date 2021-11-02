using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using Flurl;
using HappyTravel.AmazonS3Client.Extensions;
using HappyTravel.MailSender;
using HappyTravel.MailSender.Infrastructure;
using HappyTravel.MailSender.Models;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Services.Payments;
using TipCatDotNet.Api.Services.Permissions;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        // https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization
        public static AuthenticationBuilder AddAuth0Authentication(this IServiceCollection services, IConfiguration configuration)
            => services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["Auth0:Domain"];
                    options.Audience = configuration["Auth0:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });


        public static IServiceCollection AddDatabases(this IServiceCollection services, IConfiguration configuration, VaultClient vaultClient)
        {
            var databaseCredentials = vaultClient.Get(configuration["Database:Options"]).GetAwaiter().GetResult();
            return services.AddDbContextPool<AetherDbContext>(options =>
            {
                var connectionString = ConnectionStringBuilder.Build(databaseCredentials);

                options.EnableSensitiveDataLogging(false);
                options.UseNpgsql(connectionString, builder => { builder.EnableRetryOnFailure(); });
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            }, 16);
        }


        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IUserManagementClient, Auth0UserManagementClient>(c =>
                {
                    c.BaseAddress = new Uri(configuration["Auth0:Domain"]);
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient(SendGridMailSender.HttpClientName)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());

            return services;
        }


        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
        {
            var amazonS3Credentials = vaultClient.Get(configuration["AmazonS3:Options"]).GetAwaiter().GetResult();
            services.AddAmazonS3Client(options =>
            {
                options.AccessKeyId = amazonS3Credentials["accessKeyId"];
                options.DefaultBucketName = "tipcat-net";
                options.SecretKey = amazonS3Credentials["secretKey"];
                options.MaxObjectsNumberToUpload = 50;
                options.UploadConcurrencyNumber = 5;
                options.AmazonS3Config = new Amazon.S3.AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
                };
            });

            var auth0Options = vaultClient.Get(configuration["Auth0:Options"]).GetAwaiter().GetResult();
            services.Configure<Auth0ManagementApiOptions>(o =>
            {
                o.Audience = configuration["Auth0:Domain"].AppendPathSegment("/api/v2/");
                o.ClientId = auth0Options["clientId"];
                o.ClientSecret = auth0Options["clientSecret"];
                o.ConnectionId = configuration["Auth0:DatabaseId"];
            });

            services.Configure<InvitationServiceOptions>(o =>
            {
                o.TemplateId = configuration["SendGrid:EmailTemplates:MemberInvitation"];
            });
            
            var mailSettings = vaultClient.Get(configuration["SendGrid:Options"]).GetAwaiter().GetResult();
            services.Configure<SenderOptions>(options =>
            {
                options.ApiKey = mailSettings["apiKey"];
                options.BaseUrl = new Uri(configuration["SendGrid:BaseUrl"]);
                options.SenderAddress = new EmailAddress(configuration["SendGrid:SenderAddress"]);
            });

            return services;
        }


        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IMailSender, SendGridMailSender>();
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IAuthorizationHandler, MemberPermissionsAuthorizationHandler>();

            services.AddTransient<IMemberContextCacheService, MemberContextCacheService>();
            services.AddTransient<IMemberContextService, MemberContextService>();
            services.AddTransient<IPermissionChecker, PermissionChecker>();

            services.AddTransient<IQrCodeGenerator, QrCodeGenerator>();

            services.AddTransient<IInvitationService, InvitationService>();
            services.AddTransient<IFacilityService, FacilityService>();
            services.AddTransient<IMemberService, MemberService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IPaymentService, PaymentService>();

            return services;
        }


        public static IServiceCollection AddSwagger(this IServiceCollection services)
            => services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tipcat.net API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.CustomSchemaIds(x => x.FullName);

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        Array.Empty<string>()
                    }
                });
            });


        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode is System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));


        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
