using System;
using System.Threading;
using System.Threading.Tasks;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IMemberContextCacheService
    {
        ValueTask<MemberContext?> GetOrSet(string identityHash, Func<Task<MemberContext?>> getContextFunc, CancellationToken cancellationToken = default);

        void Remove(string identityHash);
    }
}
