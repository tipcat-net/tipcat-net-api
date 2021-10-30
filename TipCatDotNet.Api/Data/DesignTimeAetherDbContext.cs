using System;
using System.Collections.Generic;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.VaultClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TipCatDotNet.Api.Infrastructure;

namespace TipCatDotNet.Api.Data
{
    public class DesignTimeAetherDbContext : IDesignTimeDbContextFactory<AetherDbContext>
    {
        public AetherDbContext CreateDbContext(string[] args)
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
            var configuration = new ConfigurationBuilder()
                .AddConsulKeyValueClient(
                    Environment.GetEnvironmentVariable(Infrastructure.Constants.Common.ConsulEndpointEnvironmentVariableName) ??
                    throw new InvalidOperationException("A Consul endpoint is not set"),
                    key: Infrastructure.Constants.Common.ServiceName,
                    Environment.GetEnvironmentVariable(Infrastructure.Constants.Common.ConsulTokenEnvironmentVariableName) ??
                    throw new InvalidOperationException("A Consul http token is not set"),
                    bucketName: envName, delayOnFailureInSeconds: 60)
                .Build();
            
            var dbOptions = GetDbOptions(configuration);
            var connectionString = ConnectionStringBuilder.Build(dbOptions);

            var optionsBuilder = new DbContextOptionsBuilder<AetherDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            var context = new AetherDbContext(optionsBuilder.Options);
            context.Database.SetCommandTimeout(int.Parse(dbOptions["migrationCommandTimeout"]));

            return context;
        }
        

        private static Dictionary<string, string> GetDbOptions(IConfiguration configuration)
        {
            using var vaultClient = new VaultClient(new VaultOptions
            {
                BaseUrl = new Uri(configuration["Vault:Endpoint"]),
                Engine = configuration["Vault:Engine"],
                Role = configuration["Vault:Role"]
            });
            vaultClient.Login(Environment.GetEnvironmentVariable(Infrastructure.Constants.Common.VaultTokenEnvironmentVariableName)).Wait();

            return vaultClient.Get(configuration["Database:Options"]).Result;
        }
    }
}
