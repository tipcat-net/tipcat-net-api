using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Payments;

namespace TipCatDotNet.Api.Services.Payments
{
    public interface IPaymentService
    {
        Task<Result<PaymentDetailsResponse>> GetDetails(string memberCode, CancellationToken cancellationToken = default);

        Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest request, CancellationToken cancellationToken = default);
    }
}