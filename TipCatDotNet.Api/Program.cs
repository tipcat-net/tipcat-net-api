using System;
using System.Diagnostics;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.StdOutLogger.Infrastructure;
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
                        .UseKestrel()
                        .UseSentry(options =>
                        {
                            options.Dsn = Environment.GetEnvironmentVariable("TCDN_AETHER_SENTRY_ENDPOINT");
                            options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                            options.IncludeActivityData = true;
                            options.BeforeSend = sentryEvent =>
                            {
                                if (Activity.Current is not null)
                                    foreach (var (key, value) in Activity.Current.Baggage)
                                        sentryEvent.SetTag(key, value ?? string.Empty);

                                sentryEvent.SetTag("TraceId", Activity.Current?.TraceId.ToString() ?? string.Empty);
                                sentryEvent.SetTag("SpanId", Activity.Current?.SpanId.ToString() ?? string.Empty);

                                return sentryEvent;
                            };
                        });
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddConsulKeyValueClient(
                        Environment.GetEnvironmentVariable(Infrastructure.Constants.Common.ConsulEndpointEnvironmentVariableName) ??
                        throw new InvalidOperationException("A Consul endpoint is not set"),
                        key: Infrastructure.Constants.Common.ServiceName,
                        Environment.GetEnvironmentVariable(Infrastructure.Constants.Common.ConsulTokenEnvironmentVariableName) ??
                        throw new InvalidOperationException("A Consul http token is not set"),
                        bucketName: env.EnvironmentName, delayOnFailureInSeconds: 60);
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
                    else
                    {
                        logging.AddStdOutLogger(setup =>
                        {
                            setup.IncludeScopes = true;
                            setup.RequestIdHeader = Constants.DefaultRequestIdHeader;
                            setup.UseUtcTimestamp = true;
                        });
                        logging.AddSentry();
                    }
                });
        }
    }
}
