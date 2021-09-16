using System;
using System.IO;
using System.Reflection;
using FloxDc.CacheFlow.Extensions;
using FluentValidation.AspNetCore;
using HappyTravel.ErrorHandling.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
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
            services.AddDbContextPool<AetherDbContext>(options =>
                {
                    var connectionString = string.Format($"Server={Configuration["Database:Host"]};" +
                        $"Port={Configuration["Database:Port"]};" +
                        $"User Id={Configuration["Database:Username"]};" +
                        $"Password={Configuration["Database:Password"]};" +
                        "Database=aether;Pooling=true;");

                    options.EnableSensitiveDataLogging(false);
                    options.UseNpgsql(connectionString, builder =>
                    {
                        builder.EnableRetryOnFailure();
                    });
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
                }, 16);

            // https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/4-WebApp-your-API/4-2-B2C
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                    {
                        Configuration.Bind("AzureAdB2C", options);
                        options.TokenValidationParameters.NameClaimType = "name";
                    },
                    options => { Configuration.Bind("AzureAdB2C", options); });

            services.AddMicrosoftGraphClient(Configuration)
                .AddServices();

            services.AddControllers()
                .AddControllersAsServices();

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
