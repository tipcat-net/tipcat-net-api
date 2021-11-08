﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Auth
{
    public interface IInvitationService
    {
        Task<Result> CreateAndSend(MemberRequest request, CancellationToken cancellationToken = default);

        Task<Dictionary<int, InvitationStates>> GetState(IEnumerable<int> accountId, CancellationToken cancellationToken = bad);

        Task<Result> Redeem(int memberId, CancellationToken cancellationToken = default);

        Task<Result> Send(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);
    }
}
