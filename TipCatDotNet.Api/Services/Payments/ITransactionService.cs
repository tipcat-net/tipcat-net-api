using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Stripe;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments;

namespace TipCatDotNet.Api.Services.Payments
{
    public interface ITransactionService
    {
        Task<Result> Add(string? message, PaymentIntent paymentIntent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Method retrieve succeeded transactions by member
        /// </summary>
        Task<Result<List<TransactionResponse>>> Get(MemberContext context, int skip, int top, CancellationToken cancellationToken = default);

        Task<Result> Update(string? message, PaymentIntent paymentIntent, CancellationToken cancellationToken = default);
    }
}