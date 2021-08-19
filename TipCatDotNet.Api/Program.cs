using System;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Infrastructure;

namespace TipCatDotNet.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseKestrel();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddConsulKeyValueClient(Environment.GetEnvironmentVariable("TCDN_CONSUL_HTTP_ADDR") ??
                        throw new InvalidOperationException("Consul endpoint is not set"),
                        $"{env.EnvironmentName}/{Infrastructure.Constants.Common.ServiceName}",
                        Environment.GetEnvironmentVariable("TCDN_CONSUL_HTTP_TOKEN") ?? throw new InvalidOperationException("Consul http token is not set"),
                        delayOnFailureInSeconds: 60);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    logging.ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    
                    if (env.IsLocal())
                        logging.AddConsole();
                });
        }
    }
}
