using System;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using FloxDc.CacheFlow.Extensions;
using FluentValidation.AspNetCore;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.AmazonS3Client.Extensions;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;

namespace TipCatDotNet.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            var vaultToken = Environment.GetEnvironmentVariable("TCDN_VAULT_TOKEN")
                ?? throw new InvalidOperationException("A Vault token is not set");

            var vaultOptions = new VaultOptions
            {
                BaseUrl = new Uri(Configuration["Vault:Endpoint"]),
                Engine = Configuration["Vault:Engine"],
                Role = Configuration["Vault:Role"]
            };

            using var vaultClient = new VaultClient(vaultOptions);
            vaultClient.Login(vaultToken).GetAwaiter().GetResult();

            services.AddTransient<IVaultClient>(_ => new VaultClient(vaultOptions));

            var databaseCredentials = vaultClient.Get(Configuration["Database:Options"]).GetAwaiter().GetResult();
            services.AddDbContextPool<AetherDbContext>(options =>
            {
                var connectionString = string.Format($"Server={databaseCredentials["host"]};" +
                    $"Port={databaseCredentials["port"]};" +
                    $"User Id={databaseCredentials["username"]};" +
                    $"Password={databaseCredentials["password"]};" +
                    "Database=aether;Pooling=true;");

                options.EnableSensitiveDataLogging(false);
                options.UseNpgsql(connectionString, builder => { builder.EnableRetryOnFailure(); });
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            }, 16);

            // https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["Auth0:Domain"];
                    options.Audience = Configuration["Auth0:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            var amazonS3Credentials = vaultClient.Get(Configuration["AmazonS3:Options"]).GetAwaiter().GetResult();
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

            services.AddMicrosoftGraphClient(Configuration)
                .AddOptions(Configuration, vaultClient)
                .AddServices();

            services.AddControllers()
                .AddControllersAsServices()
                .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; });

            services.AddMemoryCache()
                .AddMemoryFlow();

            services.AddProblemDetailsErrorHandling();
            services.AddResponseCompression();

            services.AddHealthChecks()
                .AddDbContextCheck<AetherDbContext>()
                //.AddRedis(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration))
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));

            services.AddSwaggerGen(c =>
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

            services.AddMvcCore()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetCallingAssembly()));
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            app.UseProblemDetailsExceptionHandler(env, logger);

            app.UseSwagger()
                .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tipcat.net API v1"));

            app.UseHttpsRedirection();
            app.UseHsts();

            app.UseResponseCompression()
                .UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHealthChecks("/health");
                });
        }
        

        public IConfiguration Configuration { get; }
    }
}
