﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IMemberService
    {
        Task<Result<MemberResponse>> Add(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);

        Task<Result<MemberResponse>> TransferToFacility(MemberContext memberContext, int facilityId, int memberId, int accountId, CancellationToken cancellationToken = default);

        Task<Result<MemberResponse>> AddCurrent(string? identityClaim, CancellationToken cancellationToken = default);

        Task<Result<MemberResponse>> RegenerateQR(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);

        Task<Result<List<MemberResponse>>> GetAll(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);

        Task<Result<MemberResponse>> Get(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);

        Task<Result<MemberResponse>> GetCurrent(MemberContext memberContext, CancellationToken cancellationToken = default);

        Task<Result<List<MemberResponse>>> GetByFacility(MemberContext memberContext, int accountId, int facilityId, CancellationToken cancellationToken = default);

        Task<Result> Remove(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);

        Task<Result<MemberResponse>> Update(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);
    }
}
