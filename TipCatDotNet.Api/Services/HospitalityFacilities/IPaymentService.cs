using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IPaymentService
    {
        Task<Result<ReceiverResponse>> GetReceiver(string memberCode, CancellationToken cancellationToken = default);

        Task<Result<PaymentResponse>> Pay(PaymentRequest request, CancellationToken cancellationToken = default);
    }
}