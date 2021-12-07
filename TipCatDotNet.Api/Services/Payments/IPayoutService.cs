using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TipCatDotNet.Api.Services.Payments;

public interface IPayoutService
{
    Task<Result> Payout(CancellationToken cancellationToken = default);
}
