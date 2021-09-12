﻿using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IMemberService
    {
        Task<Result<MemberInfoResponse>> AddCurrent(string? id, MemberPermissions permissions, CancellationToken cancellationToken = default);
        Task<Result<MemberInfoResponse>> GetCurrent(MemberContext memberContext, CancellationToken cancellationToken = default);
        Task<Result<MemberInfoResponse>> Update(MemberContext? memberContext, MemberUpdateRequest request, CancellationToken cancellationToken = default);
        Task<Result<MemberAvatarResponse>> UpdateAvatar(int accountId, int memberId, MemberContext? memberContext,  MemberAvatarRequest request, CancellationToken cancellationToken = default);
        Task<Result<MemberInfoResponse>> VerifyEmail(MemberContext memberContext, MemberVerifyEmailRequest request, CancellationToken cancellationToken = default);
    }
}
