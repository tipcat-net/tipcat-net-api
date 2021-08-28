using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(AetherDbContext context, IMemoryFlow cache)
        {
            _context = context;
            _cache = cache;
        }


        public async ValueTask<Result> CheckEmployeePermissions(EmployeeContext employee, HospitalityFacilityPermissions permissions)
        {
            var key = _cache.BuildKey(nameof(PermissionChecker), nameof(CheckEmployeePermissions), employee.Id.ToString());

            var storedPermissions = await _cache.GetOrSetAsync(key, async () => await GetPermissions(employee.Id), EmployeePermissionsCacheLifeTime);

            return storedPermissions.Any(p => p.HasFlag(permissions))
                ? Result.Success()
                : Result.Failure(
                    $"You must have the '{permissions}' access level to use this function. Your manager may elevate you access level in the Settings section.");


            async Task<List<HospitalityFacilityPermissions>> GetPermissions(int id)
            {
                // TODO: put a database request here
                return new List<HospitalityFacilityPermissions>();
            }
        }


        private static readonly TimeSpan EmployeePermissionsCacheLifeTime = TimeSpan.FromMinutes(2);
        
        private readonly IMemoryFlow _cache;
        private readonly AetherDbContext _context;
    }
}
