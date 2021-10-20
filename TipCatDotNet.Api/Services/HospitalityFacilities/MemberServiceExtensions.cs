using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public static class MemberServiceExtensions
    {
        public static async Task<Result> EnsureMemberExists(this Result result, AetherDbContext context, int memberId,
            CancellationToken cancellationToken)
        {
            if (result.IsFailure)
                return result;

            var isMemberExist = await context.Members
                .Where(m => m.Id == memberId)
                .AnyAsync(cancellationToken);

            if (!isMemberExist)
                return Result.Failure($"The member with ID {memberId} was not found.");

            return Result.Success();
        }


        public static Result EnsureCurrentMemberBelongsToAccount(this Result result, int? currentMemberAccountId, int? targetAccountId)
        {
            if (result.IsFailure)
                return result;

            return result.Ensure(() => currentMemberAccountId == targetAccountId, "The current member does not belong to the target account.");
        }


        public static async Task<Result> EnsureTargetFacilityBelongsToAccount(this Result result, AetherDbContext context, int? facilityId, int? accountId,
            CancellationToken cancellationToken)
        {
            if (result.IsFailure)
                return result;

            var isTargetFacilityBelongsToAccount = await context.Facilities
                .Where(f => f.Id == facilityId && f.AccountId == accountId)
                .AnyAsync(cancellationToken);

            if (!isTargetFacilityBelongsToAccount)
                return Result.Failure("The target member does not belong to the target account.");

            return Result.Success();
        }
    }
}
