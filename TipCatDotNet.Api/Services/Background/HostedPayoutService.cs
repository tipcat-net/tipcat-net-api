using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Services.Background;

public class HostedPayoutService : BackgroundService
{
    public HostedPayoutService(IServiceProvider services, ILogger<HostedPayoutService> logger)
    {
        _crontabSchedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
        _services = services;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service running.");

        using (var scope = _services.CreateScope())
        {
            var timeToNext = CalculateNextExecution();
            var payoutService = scope.ServiceProvider
                    .GetRequiredService<IPayoutService>();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(timeToNext, cancellationToken);

                Console.WriteLine("PROCEED! ");
                Console.WriteLine($"Current time: {_nextRun.ToString("HH:mm:ss")}");

                // await payoutService.PayOut(cancellationToken);

                _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                timeToNext = CalculateNextExecution();
                Console.WriteLine($"Next time: {_nextRun.ToString("HH:mm:ss")}");
            }
        }
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is stopping.");

        await base.StopAsync(cancellationToken);
    }


    private int CalculateNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.UtcNow).TotalMilliseconds);


    private IServiceProvider _services { get; }
    private readonly ILogger<HostedPayoutService> _logger;
    private readonly CrontabSchedule _crontabSchedule;
    private const string Schedule = "0 0 5 * * *"; // Current schedule run task every day at 05:00:00
    // private const string Schedule = "*/10 * * * * *"; // Current schedule for debug; run task every 10 seconds
    private DateTime _nextRun = DateTime.UtcNow;
}
