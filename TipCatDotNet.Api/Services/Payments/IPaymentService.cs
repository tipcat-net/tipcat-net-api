using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Payments;
using Microsoft.Extensions.Primitives;

namespace TipCatDotNet.Api.Services.Payments;

public interface IPaymentService
{
    Task<Result> Cancel(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDetailsResponse>> Capture(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<Result<PaymentDetailsResponse>> Get(string memberCode, CancellationToken cancellationToken = default);
    Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result> ProcessChanges(string? json, StringValues headers);
    Task<Result<PaymentDetailsResponse>> Update(string paymentIntentId, PaymentRequest request, CancellationToken cancellationToken = default);
}