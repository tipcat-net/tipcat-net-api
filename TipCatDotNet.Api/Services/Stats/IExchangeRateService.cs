using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Analitics;

namespace TipCatDotNet.Api.Services.Stats;

public interface IExchangeRateService
{
    Task<Result<DataRates>> GetExchangeRates(string targetCurrency, CancellationToken cancellationToken);
}