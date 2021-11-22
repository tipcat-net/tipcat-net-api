using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Models;
using HappyTravel.Money.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Validators;
using PaymentEnums = TipCatDotNet.Api.Models.Payments.Enums;
using Stripe;

namespace TipCatDotNet.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(ITransactionService transactionService, IOptions<StripeOptions> stripeOptions, PaymentIntentService paymentIntentService, AetherDbContext context)
        {
            _stripeOptions = stripeOptions;
            _context = context;
            _paymentIntentService = paymentIntentService;
            _transactionService = transactionService;
        }


        public Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(ProceedPayment)
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, cancellationToken));


            Result Validate()
            {
                var validator = new PaymentRequestValidator(_context, cancellationToken);
                var validationResult = validator.Validate(paymentRequest);
                return validationResult.ToResult();
            }


            async Task<Result<PaymentIntent>> ProceedPayment()
            {
                var createOptions = new PaymentIntentCreateOptions
                {
                    PaymentMethodTypes = PaymentEnums.PaymentMethodService.GetAllowed(),
                    Description = "Tips",
                    Amount = ToFractionalUnits(paymentRequest.TipsAmount),
                    Currency = paymentRequest.TipsAmount.Currency.ToString(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "MemberId", paymentRequest.MemberId.ToString() },
                    },
                    CaptureMethod = "manual"
                };
                try
                {
                    var paymentIntent = await _paymentIntentService.CreateAsync(createOptions, cancellationToken: cancellationToken);
                    await _transactionService.Add(paymentIntent, cancellationToken);
                    return Result.Success(paymentIntent);
                }
                catch (StripeException ex)
                {
                    return Result.Failure<PaymentIntent>(ex.Message);
                }
            }
        }


        public Task<Result<PaymentDetailsResponse>> Update(string paymentId, PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(UpdatePayment)
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, cancellationToken));


            Result Validate()
            {
                var validator = new PaymentRequestValidator(_context, cancellationToken);
                var validationResult = validator.Validate(paymentRequest);
                return validationResult.ToResult();
            }


            async Task<Result<PaymentIntent>> UpdatePayment()
            {
                var updateOptions = new PaymentIntentUpdateOptions
                {
                    Amount = ToFractionalUnits(paymentRequest.TipsAmount),
                    Currency = paymentRequest.TipsAmount.Currency.ToString()
                };
                try
                {
                    var paymentIntent = await _paymentIntentService.UpdateAsync(paymentId, updateOptions, cancellationToken: cancellationToken);
                    await _transactionService.Update(paymentIntent, cancellationToken);
                    return Result.Success(paymentIntent);
                }
                catch (StripeException ex)
                {
                    return Result.Failure<PaymentIntent>(ex.Message);
                }
            }
        }


        public Task<Result> Cancel(string paymentId, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Bind(CancelPayment);


            async Task<Result> CancelPayment()
            {
                var cancelOptions = new PaymentIntentCancelOptions
                {
                    CancellationReason = "requested_by_customer"
                };
                try
                {
                    var paymentIntent = await _paymentIntentService.CancelAsync(paymentId, cancelOptions, cancellationToken: cancellationToken);
                    return Result.Success();
                }
                catch (StripeException ex)
                {
                    return Result.Failure(ex.Message);
                }
            }
        }


        public Task<Result<PaymentDetailsResponse>> Capture(string paymentIntentId, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => CapturePayment(paymentIntentId, cancellationToken))
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, cancellationToken));


        public Task<Result<PaymentDetailsResponse>> GetPreparationDetails(string memberCode, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => GetPaymentDetails(null, memberCode, cancellationToken));


        public Task<Result<PaymentDetailsResponse>> Get(string paymentIntentId, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => GetPaymentIntent(paymentIntentId, cancellationToken))
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, cancellationToken));


        public Task<Result> ProcessChanges(string? json, StringValues headers)
        {
            return Result.Success()
                .Bind(() => DefineEventType(json, headers))
                .Bind(CallMethod);


            Result<Event> DefineEventType(string? json, StringValues headers)
            {
                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(
                        json,
                        headers,
                        _stripeOptions.Value.WebhookSecret
                    );

                    return stripeEvent;
                }
                catch (StripeException se)
                {
                    return Result.Failure<Event>(se.Message);
                }
            }

            async Task<Result> CallMethod(Event stripeEvent)
            {
                var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;

                switch (stripeEvent.Type)
                {
                    case "payment_intent.created":
                        {
                            // TODO: call method for handle created event
                            break;
                        };
                    case "payment_intent.succeeded":
                        {
                            await _transactionService.Update(paymentIntent!);
                            break;
                        }
                    default:
                        break;
                }

                return Result.Success();
            }

        }


        private async Task<Result<PaymentIntent>> GetPaymentIntent(string paymentIntentId, CancellationToken cancellationToken)
        {
            try
            {
                var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
                if (paymentIntent.Object != null)
                    return paymentIntent;

                return Result.Failure<PaymentIntent>($"The payment intent with ID {paymentIntentId} was not found.");
            }
            catch (StripeException ex)
            {
                return Result.Failure<PaymentIntent>(ex.Message);
            }
        }


        private async Task<Result<PaymentIntent>> CapturePayment(string paymentIntentId, CancellationToken cancellationToken)
        {
            try
            {
                var option = new PaymentIntentCaptureOptions
                {
                    // TODO
                    // ApplicationFeeAmount = ((long)Math.Ceiling(amount * feeRate))
                };

                var paymentIntent = await _paymentIntentService.CaptureAsync(paymentIntentId, option, cancellationToken: cancellationToken);

                if (paymentIntent != null)
                    return paymentIntent;

                return Result.Failure<PaymentIntent>($"The payment intent with ID {paymentIntentId} was not found.");
            }
            catch (StripeException ex)
            {
                return Result.Failure<PaymentIntent>(ex.Message);
            }
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


        private async Task<Result<PaymentDetailsResponse>> GetPaymentDetails(PaymentIntent paymentIntent, CancellationToken cancellationToken)
        {
            if (!paymentIntent.Metadata.ContainsKey("MemberId"))
                return Result.Failure<PaymentDetailsResponse>($"The paymentIntent does not containt member's metadata.");

            var memberId = int.Parse(paymentIntent.Metadata["MemberId"]);

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


        private long ToFractionalUnits(in MoneyAmount tipsAmount)
            => (long)(tipsAmount.Amount * (decimal)Math.Pow(10, tipsAmount.Currency.GetDecimalDigitsCount()));


        private readonly AetherDbContext _context;

        private readonly PaymentIntentService _paymentIntentService;

        private readonly IOptions<StripeOptions> _stripeOptions;

        private readonly ITransactionService _transactionService;
    }
}