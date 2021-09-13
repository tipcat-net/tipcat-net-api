using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class MemberContextService : IMemberContextService
    {
        public MemberContextService(AetherDbContext context, IHttpContextAccessor httpContextAccessor, IMemberContextCacheService cache)
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

            return await _cache.GetOrSet(identityHash, async () => await GetContextInfoByIdentityHash(identityHash));
        }


        private async Task<MemberContext?> GetContextInfoByIdentityHash(string identityHash)
            => await _context.Members.GroupJoin(_context.AccountMembers, m => m.Id, am => am.MemberId, (m, grouping) => new { m, grouping })
                .SelectMany(@t => @t.grouping.DefaultIfEmpty(), (@t, g) => new { @t, g })
                .Where(@t => @t.@t.m.IdentityHash == identityHash)
                .Select(@t => new MemberContext(@t.@t.m.Id, @t.g.AccountId, @t.@t.m.Email))
                .SingleOrDefaultAsync();


        private MemberContext? _memberContext;
        
        private readonly IMemberContextCacheService _cache;
        private readonly AetherDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
    }
}
