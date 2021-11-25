using System;
using System.Reflection;
using System.Text.Json;
using FloxDc.CacheFlow.Extensions;
using FluentValidation.AspNetCore;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.Money.Enums;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Payments.Validators;

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
            using var vaultClient = GetVaultClient(services);

            services.AddAuth0Authentication(Configuration);

            services.AddStripe(Configuration, vaultClient);

            services.AddDatabases(Configuration, vaultClient)
                .AddOptions(Configuration, vaultClient)
                .AddHttpClients(Configuration)
                .AddServices();

            services.AddMemoryCache()
                .AddMemoryFlow()
                .AddProblemDetailsErrorHandling()
                .AddResponseCompression();

            services.AddHealthChecks()
                .AddDbContextCheck<AetherDbContext>()
                //.AddRedis(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration))
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));

            services.AddControllers()
                .AddControllersAsServices()
                .AddJsonOptions(options =>
                {
                    // Add converters that ovveride base logic for autoserialize Enum through endpoint request
                    // options.JsonSerializerOptions.Converters.Add(new StringEnumValidator<Currencies>());
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            services.AddSwagger()
                .AddMvcCore()
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


        private VaultClient GetVaultClient(IServiceCollection services)
        {
            var vaultToken = Environment.GetEnvironmentVariable(Infrastructure.Constants.Common.VaultTokenEnvironmentVariableName)
                ?? throw new InvalidOperationException("A Vault token is not set");

            var vaultOptions = new VaultOptions
            {
                BaseUrl = new Uri(Configuration["Vault:Endpoint"]),
                Engine = Configuration["Vault:Engine"],
                Role = Configuration["Vault:Role"]
            };

            var vaultClient = new VaultClient(vaultOptions);
            vaultClient.Login(vaultToken).GetAwaiter().GetResult();

            services.AddTransient<IVaultClient>(_ => new VaultClient(vaultOptions));
            return vaultClient;
        }


        public IConfiguration Configuration { get; }
    }
}
