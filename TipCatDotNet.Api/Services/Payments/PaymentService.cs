using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Validators;
using Stripe;

namespace TipCatDotNet.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(IOptions<PaymentSettings> paymentSettings, AetherDbContext context)
        {
            _paymentSettings = paymentSettings.Value;
            _context = context;
        }


        public Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(ProceedPayment)
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, paymentRequest.MemberId, cancellationToken));


            Result Validate()
            {
                var validator = new PaymentRequestValidator(_context, cancellationToken);
                var validationResult = validator.Validate(paymentRequest);
                return validationResult.ToResult();
            }


            async Task<Result<PaymentIntent>> ProceedPayment()
            {
                var service = new PaymentIntentService();
                var createOptions = new PaymentIntentCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    Amount = ((long)paymentRequest.TipsAmount.Amount), // TODO: get valid amount from payment request
                    Currency = paymentRequest.TipsAmount.Currency.ToString(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "MemberId", paymentRequest.MemberId.ToString() },
                    }
                };

                var requestOptions = new RequestOptions();
                requestOptions.ApiKey = _paymentSettings.StripePrivateKey;
                requestOptions.IdempotencyKey = "SOME STRING"; // An idempotency key is a unique value generated by the client which the server uses to recognize subsequent retries of the same request.
                requestOptions.StripeAccount = "CONNECTED ACCOUNT ID"; // I think we need get it from

                var paymentIntent = await service.CreateAsync(createOptions, requestOptions, cancellationToken);

                if (paymentIntent.Status == "requires_payment_method")
                    Result.Failure($"Payment declined.");

                return Result.Success(paymentIntent);
            }
        }


        public Task<Result<PaymentDetailsResponse>> GetMemberDetails(string memberCode, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => GetPaymentDetails(null, memberCode, cancellationToken));


        public Task<Result<PaymentDetailsResponse>> Get(string paymentIntentId, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => GetPaymentIntent(paymentIntentId, cancellationToken))
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, int.Parse(paymentIntent.Metadata["MemberId"]), cancellationToken));


        private async Task<Result<PaymentIntent>> GetPaymentIntent(string paymentIntentId, CancellationToken cancellationToken)
        {
            StripeConfiguration.ApiKey = _paymentSettings.StripePrivateKey;

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            if (paymentIntent != null)
                return paymentIntent;

            return Result.Failure<PaymentIntent>($"The payment intent with ID {paymentIntentId} was not found.");
        }


        private async Task<Result<PaymentDetailsResponse>> GetPaymentDetails(PaymentIntent? paymentIntent, string memberCode, CancellationToken cancellationToken)
        {
            var paymentDetails = await _context.Members
                .Where(m => m.MemberCode == memberCode)
                .Select(MemberInfoProjection())
                .Select(PaymentDetailsProjection(paymentIntent))
                .SingleOrDefaultAsync(cancellationToken);

            if (!paymentDetails.Equals(default))
                return paymentDetails;

            return Result.Failure<PaymentDetailsResponse>($"The member with MemberCode {memberCode} was not found.");
        }


        private async Task<Result<PaymentDetailsResponse>> GetPaymentDetails(PaymentIntent paymentIntent, int memberId, CancellationToken cancellationToken)
        {
            var paymentDetails = await _context.Members
                .Where(m => m.Id == memberId)
                .Select(MemberInfoProjection())
                .Select(PaymentDetailsProjection(paymentIntent))
                .SingleOrDefaultAsync(cancellationToken);

            if (!paymentDetails.Equals(default))
                return paymentDetails;

            return Result.Failure<PaymentDetailsResponse>($"The member with ID {memberId} was not found.");
        }


        private Expression<Func<Member, PaymentDetailsResponse.MemberInfo>> MemberInfoProjection()
                => member => new PaymentDetailsResponse.MemberInfo(member.Id, member.FirstName, member.LastName, member.AvatarUrl);


        private Expression<Func<PaymentDetailsResponse.MemberInfo, PaymentDetailsResponse>> PaymentDetailsProjection(PaymentIntent? paymentIntent)
                => memberInfo => new PaymentDetailsResponse(memberInfo, paymentIntent);


        private readonly AetherDbContext _context;

        private readonly PaymentSettings _paymentSettings;
    }
}