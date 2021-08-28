using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using Microsoft.AspNetCore.Http;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class EmployeeContextService : IEmployeeContextService
    {
        public EmployeeContextService(AetherDbContext context, IHttpContextAccessor httpContextAccessor, IMemoryFlow cache)
        {
            _cache = cache;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        public async ValueTask<Result<EmployeeContext>> GetInfo()
        {
            if (_employeeContext != default)
                return _employeeContext;

            _employeeContext = await GetContext();

            return _employeeContext ?? Result.Failure<EmployeeContext>("Unable to get employee context.");
        }


        private async ValueTask<EmployeeContext?> GetContext()
        {
            var identityClaim = GetClaimValue("sub");
            var identityHash = identityClaim != default
                ? HashGenerator.ComputeSha256(identityClaim)
                : string.Empty;

            var key = _cache.BuildKey(nameof(EmployeeContextService), nameof(GetContext), identityHash);

            return await _cache.GetOrSetAsync(key, async () => await GetContextInfoByIdentityHash(identityHash), EmployeeContextCacheLifeTime);
        }


        private string? GetClaimValue(string claimType)
            => _httpContextAccessor.HttpContext?
                .User
                .Claims
                .SingleOrDefault(c => c.Type == claimType)?.Value;


        private async Task<EmployeeContext?> GetContextInfoByIdentityHash(string identityHash)
        {
            // TODO: put a database request here
            throw new NotImplementedException();
        }


        private EmployeeContext? _employeeContext;
        private static readonly TimeSpan EmployeeContextCacheLifeTime = TimeSpan.FromMinutes(2);

        private readonly IMemoryFlow _cache;
        private readonly AetherDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
    }
}
