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


        public Task<Result<PaymentResponse>> Pay(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .EnsureReceiverExists(_context, paymentRequest.MemberCode, cancellationToken)
                .Bind(() => ProceedPayment());

            async Task<Result<PaymentResponse>> ProceedPayment()
            {
                //TODO: use payment gateway API's for proceed payment
                return Result.Failure<PaymentResponse>($"Payment declined.");
            }
        }


        public Task<Result<ReceiverResponse>> GetReceiver(string memberCode, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(() => GetReceiverInfo(memberCode, cancellationToken));


        private async Task<Result<ReceiverResponse>> GetReceiverInfo(string memberCode, CancellationToken cancellationToken)
        {
            var receiver = await _context.Members
                .Where(m => m.MemberCode == memberCode)
                .Select(ReceiverProjection())
                .SingleOrDefaultAsync(cancellationToken);

            if (!receiver.Equals(default))
                return receiver;

            return Result.Failure<ReceiverResponse>($"The receiver with MemberCode {memberCode} was not found.");
        }


        private static Expression<Func<Member, ReceiverResponse>> ReceiverProjection()
            => member => new ReceiverResponse(member.MemberCode);


        private readonly AetherDbContext _context;

        private readonly ILogger<MemberService> _logger;
    }
}