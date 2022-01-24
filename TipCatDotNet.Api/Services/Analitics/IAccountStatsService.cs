using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data.Models.Payment;

namespace TipCatDotNet.Api.Services.Analitics;

public interface IAccountStatsService
{
    Task AddOrUpdate(Transaction transaction, CancellationToken cancellationToken = default);
}