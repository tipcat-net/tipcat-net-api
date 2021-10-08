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


        public static async Task<Result> EnsureTargetAccountHasNoDefault(this Result result, AetherDbContext context, int? targetAccountId,
            CancellationToken cancellationToken)
        {
            if (result.IsFailure)
                return result;

            var isDefaultFacilityExist = await context.Facilities
                .Where(f => f.AccountId == targetAccountId && f.IsDefault == true)
                .AnyAsync(cancellationToken);

            if (isDefaultFacilityExist)
                return Result.Failure("The target account already has default facility.");

            return Result.Success();
        }


        public static async Task<Result> EnsureTargetFacilityNotEqualMembersOne(this Result result, AetherDbContext context, int? memberId, int? targetFacilityId,
            CancellationToken cancellationToken)
        {
            if (result.IsFailure)
                return result;

            var isEquivalentFacilities = await context.Members
                .Where(m => m.Id == memberId && m.FacilityId == targetFacilityId)
                .AnyAsync(cancellationToken);

            if (isEquivalentFacilities)
                return Result.Failure("The target account already has default facility.");

            return Result.Success();
        }


        public static async Task<Result> EnsureTargetMemberBelongsToAccount(this Result result, AetherDbContext context, int? memberId, int? accountId,
            CancellationToken cancellationToken)
        {
            if (result.IsFailure)
                return result;

            var isRequestedMemberBelongsToAccount = await context.Members
                .Where(m => m.Id == memberId && m.AccountId == accountId)
                .AnyAsync(cancellationToken);

            if (!isRequestedMemberBelongsToAccount)
                return Result.Failure("The target member does not belong to the target account.");

            return Result.Success();
        }
    }
}
