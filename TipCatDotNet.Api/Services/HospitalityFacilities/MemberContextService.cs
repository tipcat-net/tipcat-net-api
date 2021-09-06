using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class MemberContextService : IMemberContextService
    {
        public MemberContextService(AetherDbContext context, IHttpContextAccessor httpContextAccessor, IMemoryFlow cache)
        {
            _cache = cache;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        public async ValueTask<Result<MemberContext>> Get()
        {
            if (_memberContext != default)
                return _memberContext;

            _memberContext = await GetContext();

            return _memberContext ?? Result.Failure<MemberContext>("Unable to get member context.");
        }


        private async ValueTask<MemberContext?> GetContext()
        {
            var identityClaim = _httpContextAccessor.HttpContext?.User.GetId();
            var identityHash = identityClaim is not null
                ? HashGenerator.ComputeSha256(identityClaim)
                : string.Empty;

            var key = _cache.BuildKey(nameof(MemberContextService), nameof(GetContext), identityHash);

            return await _cache.GetOrSetAsync(key, async () => await GetContextInfoByIdentityHash(identityHash), MemberContextCacheLifeTime);
        }


        private async Task<MemberContext?> GetContextInfoByIdentityHash(string identityHash)
            => await _context.Members
                .Where(m => m.IdentityHash == identityHash)
                .Select(m => new MemberContext(m.Id, m.Email))
                .SingleOrDefaultAsync();


        private MemberContext? _memberContext;
        private static readonly TimeSpan MemberContextCacheLifeTime = TimeSpan.FromMinutes(2);

        private readonly IMemoryFlow _cache;
        private readonly AetherDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
    }
}
