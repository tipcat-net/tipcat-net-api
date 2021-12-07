using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TipCatDotNet.Api.Services.Payments;

public interface IPayoutService
{
    Task<Result> PayoutAll(CancellationToken cancellationToken = default);
}
