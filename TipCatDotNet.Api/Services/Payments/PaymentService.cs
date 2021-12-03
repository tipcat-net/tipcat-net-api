using System;
using System.Collections.Generic;
using System.Linq;
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
        public PaymentService(AetherDbContext context, ITransactionService transactionService, IOptions<StripeOptions> stripeOptions,
            PaymentIntentService paymentIntentService, IProFormaInvoiceService proFormaInvoiceService)
        {
            _context = context;
            _paymentIntentService = paymentIntentService;
            _proFormaInvoiceService = proFormaInvoiceService;
            _stripeOptions = stripeOptions;
            _transactionService = transactionService;
        }


        public Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(GetOperatingName)
                .Bind(ProceedPayment)
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, cancellationToken));


            Result Validate()
            {
                var validator = new PaymentRequestValidator(_context, cancellationToken);
                var validationResult = validator.Validate(paymentRequest);
                return validationResult.ToResult();
            }


            async Task<Result<string>> GetOperatingName()
                => await _context.Members
                    .Where(m => m.Id == paymentRequest.MemberId)
                    .Join(_context.Accounts, m => m.AccountId, a => a.Id, (m, a) => a.OperatingName)
                    .SingleAsync();


            async Task<Result<PaymentIntent>> ProceedPayment(string operatingName)
            {
                var createOptions = new PaymentIntentCreateOptions
                {
                    PaymentMethodTypes = PaymentEnums.PaymentMethodService.GetAllowed(),
                    Description = $"Tips left at {operatingName}",
                    Amount = ToIntegerUnits(paymentRequest.TipsAmount),
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
                    await _transactionService.Add(paymentIntent, paymentRequest.Message, cancellationToken);
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
                    Amount = ToIntegerUnits(paymentRequest.TipsAmount),
                    Currency = paymentRequest.TipsAmount.Currency.ToString()
                };
                try
                {
                    var paymentIntent = await _paymentIntentService.UpdateAsync(paymentId, updateOptions, cancellationToken: cancellationToken);
                    await _transactionService.Update(paymentIntent, paymentRequest.Message, cancellationToken);
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


        public Task<Result<PaymentDetailsResponse>> Get(string memberCode, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => GetPaymentDetails(memberCode, cancellationToken));


        public Task<Result> ProcessChanges(string? json, StringValues headers)
        {
            return Result.Success()
                .Bind(DefineEventType)
                .Bind(PerformAction);


            Result<Event> DefineEventType()
            {
                try
                {
                    return EventUtility.ConstructEvent(json, headers, _stripeOptions.Value.WebhookSecret);
                }
                catch (StripeException se)
                {
                    return Result.Failure<Event>(se.Message);
                }
            }

            async Task<Result> PerformAction(Event stripeEvent)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                switch (stripeEvent.Type)
                {
                    case "payment_intent.created":
                        {
                            // TODO: call method for handle created event
                            break;
                        }
                    case "payment_intent.succeeded":
                        {
                            await _transactionService.Update(paymentIntent!, null);
                            break;
                        }
                }

                return Result.Success();
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

                return paymentIntent ?? Result.Failure<PaymentIntent>($"The payment intent with ID {paymentIntentId} was not found.");
            }
            catch (StripeException ex)
            {
                return Result.Failure<PaymentIntent>(ex.Message);
            }
        }


        private async Task<Result<PaymentDetailsResponse>> GetPaymentDetails(string memberCode, CancellationToken cancellationToken)
        {
            var memberQuery = _context.Members
                .Where(m => m.MemberCode == memberCode);

            return await GetPaymentDetailsInternal(memberQuery, paymentIntent: null, cancellationToken);
        }


        private async Task<Result<PaymentDetailsResponse>> GetPaymentDetails(PaymentIntent paymentIntent, CancellationToken cancellationToken)
        {
            if (!paymentIntent.Metadata.ContainsKey("MemberId"))
                return Result.Failure<PaymentDetailsResponse>("The paymentIntent does not contain member's metadata.");

            var memberId = int.Parse(paymentIntent.Metadata["MemberId"]);
            var memberQuery = _context.Members
                .Where(m => m.Id == memberId);

            return await GetPaymentDetailsInternal(memberQuery, paymentIntent, cancellationToken);
        }


        private async Task<Result<PaymentDetailsResponse>> GetPaymentDetailsInternal(IQueryable<Member> memberQuery, PaymentIntent? paymentIntent, CancellationToken cancellationToken)
        {
            var memberInfo = await GetMemberInfoAsQueryable(memberQuery)
                .SingleOrDefaultAsync(cancellationToken);

            if (memberInfo.Equals(default))
                return Result.Failure<PaymentDetailsResponse>("The member was not found.");

            var proFormaInvoice = await _proFormaInvoiceService.Get(cancellationToken);

            return new PaymentDetailsResponse(memberInfo, proFormaInvoice, paymentIntent);


            IQueryable<PaymentDetailsResponse.MemberInfo> GetMemberInfoAsQueryable(IQueryable<Member> member)
                => member
                    .Join(_context.Facilities, m => m.FacilityId, f => f.Id, (m, f) => new { m, f })
                    .Join(_context.Accounts, x => x.m.AccountId, a => a.Id, (x, a) => new { x.m, x.f, a })
                    .Select(x => new PaymentDetailsResponse.MemberInfo(x.m.Id, x.m.FirstName, x.m.LastName, x.m.Position, x.m.AvatarUrl, x.a.OperatingName, x.f.Name));
        }


        private static long ToIntegerUnits(in MoneyAmount tipsAmount)
            => (long)(tipsAmount.Amount * (decimal)Math.Pow(10, tipsAmount.Currency.GetDecimalDigitsCount()));


        private readonly AetherDbContext _context;
        private readonly PaymentIntentService _paymentIntentService;
        private readonly IProFormaInvoiceService _proFormaInvoiceService;
        private readonly IOptions<StripeOptions> _stripeOptions;
        private readonly ITransactionService _transactionService;
    }
}