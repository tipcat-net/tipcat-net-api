using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.Analitics;

namespace TipCatDotNet.Api.Services.Stats;

public class ExchangeRateService : IExchangeRateService
{
    public ExchangeRateService(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("striperates.com");
        _logger = loggerFactory.CreateLogger<ExchangeRateService>();
    }


    public async Task<Result<DataRates>> GetExchangeRates(string targetCurrency, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"/rates/{targetCurrency}.alt", cancellationToken);

            response.EnsureSuccessStatusCode();
            var ratesResponse = await JsonSerializer.DeserializeAsync<RatesResponse>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            var dataRates = ratesResponse.Data.First();

            return Result.Success(dataRates);
        }
        catch (HttpRequestException)
        {
            var message = $"Exchanging rates for {targetCurrency} occurred an exception throw request!";
            _logger.LogExchangeRateException(message);
            return Result.Failure<DataRates>(message);
        }
    }


    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeRateService> _logger;
}