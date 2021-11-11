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
using TipCatDotNet.Api.Models.Payments.Enums;
using Stripe;

namespace TipCatDotNet.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(IOptions<PaymentSettings> paymentSettings, AetherDbContext context)
        {
            _paymentSettings = paymentSettings.Value;
            _context = context;
            _client = new StripeClient(paymentSettings.Value.StripePrivateKey);
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
                const double feeRate = 0.05;
                var amount = Int64.Parse(paymentRequest.TipsAmount.Amount.ToString().Replace(".",""));

                var createOptions = new PaymentIntentCreateOptions
                {
                    PaymentMethodTypes = AllowedPaymentMethods.ToList(),
                    Description = "Tips",
                    Amount = amount,
                    ApplicationFeeAmount = ((long)Math.Ceiling(amount * feeRate)),
                    Currency = paymentRequest.TipsAmount.Currency.ToString(),
                    Metadata = new Dictionary<string, string>
                    {
                        { "MemberId", paymentRequest.MemberId.ToString() },
                    },
                    CaptureMethod = "manual"
                };
                var service = new PaymentIntentService(_client);
                try
                {
                    var paymentIntent = await service.CreateAsync(createOptions, null, cancellationToken);
                    return Result.Success(paymentIntent);
                }
                catch (StripeException ex)
                {
                    return Result.Failure<PaymentIntent>(ex.Message);
                }
            }
        }

        
        public Task<Result<PaymentDetailsResponse>> Capture(string paymentIntentId, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => CapturePayment(paymentIntentId, cancellationToken))
                .Bind(paymentIntent => GetPaymentDetails(paymentIntent, int.Parse(paymentIntent.Metadata["MemberId"]), cancellationToken));


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
            var paymentIntent = await service.GetAsync(paymentIntentId, null , null, cancellationToken);

            if (paymentIntent != null)
                return paymentIntent;

            return Result.Failure<PaymentIntent>($"The payment intent with ID {paymentIntentId} was not found.");
        }


        private async Task<Result<PaymentIntent>> CapturePayment(string paymentIntentId, CancellationToken cancellationToken)
        {
            var service = new PaymentIntentService(_client);

            try
            {                 
                var paymentIntent = await service.CaptureAsync(paymentIntentId, null, null, cancellationToken);
                
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

        private readonly IStripeClient _client;
    }
}