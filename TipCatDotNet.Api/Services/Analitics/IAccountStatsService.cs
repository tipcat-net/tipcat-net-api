using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Models.Analitics;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Analitics;

public interface IAccountStatsService
{
    Task AddOrUpdate(Transaction transaction, CancellationToken cancellationToken = default);
    Task<Result<AccountStatsResponse>> Get(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default);
    Task<List<FacilityStatsResponse>> GetFacilities(int accountId, CancellationToken cancellationToken = default);
}