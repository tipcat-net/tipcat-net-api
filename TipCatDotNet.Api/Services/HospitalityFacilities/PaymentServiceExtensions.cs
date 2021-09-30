using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public static class PaymentServiceExtensions
    {
        public static async Task<Result> EnsureReceiverExists(this Result result, AetherDbContext context, string memberCode,
            CancellationToken cancellationToken)
        {
            if (result.IsFailure)
                return result;

            var isExistedReceiver = await context.Members
                .Where(m => m.MemberCode == memberCode)
                .AnyAsync(cancellationToken);

            if (!isExistedReceiver)
                return Result.Failure($"The receiver with MemberCode {memberCode} was not found.");

            return Result.Success();
        }
    }
}