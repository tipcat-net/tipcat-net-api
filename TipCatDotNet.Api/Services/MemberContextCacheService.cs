using System;
using System.Threading;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Services;

public class MemberContextCacheService : IMemberContextCacheService
{
    public MemberContextCacheService(IMemoryFlow cache)
    {
        _cache = cache;
    }


    public ValueTask<MemberContext?> GetOrSet(string identityHash, Func<Task<MemberContext?>> getContextFunc, CancellationToken cancellationToken = default) 
        => _cache.GetOrSetAsync(BuildKey(identityHash), getContextFunc, MemberContextCacheLifeTime, cancellationToken);


    public void Remove(string identityHash) 
        => _cache.Remove(BuildKey(identityHash));


    private string BuildKey(string identityHash) 
        => _cache.BuildKey(nameof(MemberContextCacheService), identityHash);


    private static readonly TimeSpan MemberContextCacheLifeTime = TimeSpan.FromMinutes(2);
        
    private readonly IMemoryFlow _cache;
}