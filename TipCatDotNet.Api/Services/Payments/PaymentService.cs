using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Validators;

namespace TipCatDotNet.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(AetherDbContext context)
        {
            _context = context;
        }


        public Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(ProceedPayment);


            Result Validate()
            {
                var validator = new PaymentRequestValidator(_context, cancellationToken);
                var validationResult = validator.Validate(paymentRequest);
                return validationResult.ToResult();
            }


            async Task<Result<PaymentDetailsResponse>> ProceedPayment()
            {
                //TODO: use payment gateway API for proceed payment
                return Result.Failure<PaymentDetailsResponse>($"Payment declined.");
            }
        }


        public Task<Result<PaymentDetailsResponse>> GetDetails(string memberCode, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Bind(GetPaymentDetails);


            async Task<Result<PaymentDetailsResponse>> GetPaymentDetails()
            {
                var paymentDetails = await _context.Members
                    .Where(m => m.MemberCode == memberCode)
                    .Select(PaymentDetailsProjection())
                    .SingleOrDefaultAsync(cancellationToken);

                if (!paymentDetails.Equals(default))
                    return paymentDetails;

                return Result.Failure<PaymentDetailsResponse>($"The member with MemberCode {memberCode} was not found.");
            }


            Expression<Func<Member, PaymentDetailsResponse>> PaymentDetailsProjection()
                => member => new PaymentDetailsResponse(member.Id, member.FirstName, member.LastName, member.AvatarUrl);
        }


        private readonly AetherDbContext _context;
    }
}