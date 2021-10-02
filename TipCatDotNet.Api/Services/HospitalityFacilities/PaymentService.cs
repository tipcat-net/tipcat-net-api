using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;


namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(ILoggerFactory loggerFactory, AetherDbContext context)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<MemberService>();
        }


        public Task<Result<PaymentDetailsResponse>> Pay(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .EnsureMemberExists(_context, paymentRequest.MemberId, cancellationToken)
                .Bind(() => ProceedPayment());

            async Task<Result<PaymentDetailsResponse>> ProceedPayment()
            {
                //TODO: use payment gateway API's for proceed payment
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

        private readonly ILogger<MemberService> _logger;
    }
}