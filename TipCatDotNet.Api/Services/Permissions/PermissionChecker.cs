using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Permissions
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(AetherDbContext context, IMemoryFlow cache)
        {
            _context = context;
            _cache = cache;
        }


        public async ValueTask<Result> CheckMemberPermissions(MemberContext member, MemberPermissions permissions,
            CancellationToken cancellationToken = default)
        {
            var key = _cache.BuildKey(nameof(PermissionChecker), nameof(CheckMemberPermissions), member.Id.ToString());

            var storedPermissions = await _cache
                .GetOrSetAsync(key, async () => await GetPermissions(member.Id), MemberPermissionsCacheLifeTime, cancellationToken);

            if (storedPermissions == MemberPermissions.None)
                return Result.Failure($"You must have any permission to use this function. For now you have none.");

            return permissions.HasFlag(storedPermissions)
                ? Result.Success()
                : Result.Failure(
                    $"You must have the '{permissions}' access level to use this function. Your manager may elevate you access level in the Settings section.");


            async Task<MemberPermissions> GetPermissions(int id)
                => await _context.Members
                    .Where(m => m.Id == id)
                    .Select(m => m.Permissions)
                    .SingleOrDefaultAsync(cancellationToken);
        }


        private static readonly TimeSpan MemberPermissionsCacheLifeTime = TimeSpan.FromMinutes(2);
        
        private readonly IMemoryFlow _cache;
        private readonly AetherDbContext _context;
    }
}
