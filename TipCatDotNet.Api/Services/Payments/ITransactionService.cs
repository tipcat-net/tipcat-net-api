using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Stripe;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Enums;

namespace TipCatDotNet.Api.Services.Payments;

public interface ITransactionService
{
    Task<Result> Add(PaymentIntent paymentIntent, string? message, CancellationToken cancellationToken = default);
    /// <summary>
    /// Method retrieve succeeded transactions by member
    /// </summary>
    Task<Result<List<TransactionResponse>>> Get(MemberContext context, int skip, int top, TransactionFilterProperty property, CancellationToken cancellationToken = default);
    Task<Result> Update(PaymentIntent paymentIntent, string? message, CancellationToken cancellationToken = default);
}