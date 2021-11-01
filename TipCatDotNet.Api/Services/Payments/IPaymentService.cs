using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Payments;

namespace TipCatDotNet.Api.Services.Payments
{
    public interface IPaymentService
    {
        Task<Result<PaymentDetailsResponse>> GetMemberDetails(string memberCode, CancellationToken cancellationToken = default);

        Task<Result<PaymentDetailsResponse>> Get(string paymentIntentId, CancellationToken cancellationToken = default);

        Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest request, CancellationToken cancellationToken = default);

        Task<Result<PaymentDetailsResponse>> Capture(string paymentIntentId, CancellationToken cancellationToken = default);
    }
}