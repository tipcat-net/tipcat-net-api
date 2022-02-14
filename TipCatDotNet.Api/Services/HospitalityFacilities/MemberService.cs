using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Infrastructure.FunctionalExtensions;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.Images;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Services.HospitalityFacilities;

public class MemberService : IMemberService
{
    public MemberService(IStripeAccountService stripeAccountService, ILoggerFactory loggerFactory, AetherDbContext context, IUserManagementClient userManagementClient,
        IQrCodeGenerator qrCodeGenerator, IInvitationService invitationService)
    {
        _stripeAccountService = stripeAccountService;
        _context = context;
        _invitationService = invitationService;
        _logger = loggerFactory.CreateLogger<MemberService>();
        _qrCodeGenerator = qrCodeGenerator;
        _userManagementClient = userManagementClient;
    }


    public Task<Result<MemberResponse>> Activate(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(ActivateInternal)
            .Bind(memberId => GetMember(memberId, cancellationToken));


        Result Validate()
        {
            var validator = new MemberRequestValidator(memberContext, _context);
            return validator.ValidateChangeState(request).ToResult();
        }


        Task<Result<int>> ActivateInternal()
            => ChangeState(request.Id!.Value, true, cancellationToken);
    }


    public Task<Result<MemberResponse>> Add(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .BindWithTransaction(_context,
                () => AddMemberInternal(string.Empty, request.AccountId, request.FirstName, request.LastName, request.Permissions, request.Email,
                        request.Position, cancellationToken)
                    .Bind(SendInvitation)
                    .Bind(memberId => AssignMemberCode(memberId, cancellationToken))
                    .Bind(memberId => GetMember(memberId, cancellationToken)));


        Result Validate()
        {
            var validator = new MemberRequestValidator(memberContext, _context);
            return validator.ValidateAdd(request).ToResult();
        }


        async Task<Result<int>> SendInvitation(int memberId)
        {
            var modifiedRequest = new MemberRequest(memberId, in request);
            var (_, isFailure, error) = await _invitationService.CreateAndSend(modifiedRequest, cancellationToken);
            return isFailure
                ? Result.Failure<int>(error)
                : memberId;
        }
    }


    public Task<Result<MemberResponse>> AddCurrent(string? identityClaim, CancellationToken cancellationToken = default)
    {
        return Result.Success()
            .Ensure(() => identityClaim is not null, "The provided Jwt token contains no IDs. Highly likely this is a security configuration issue.")
            .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
            .Bind(ComputeHash)
            .Ensure(async identityHash => !await CheckIfMemberAlreadyAdded(identityHash), "Another user was already added from this token data.")
            .Bind(GetUserContext)
            .Bind(AddMember);


        async Task<bool> CheckIfMemberAlreadyAdded(string identityHash)
            => await _context.Members
                .Where(m => m.IdentityHash == identityHash)
                .AnyAsync(cancellationToken);


        Result<string> ComputeHash() => HashGenerator.ComputeSha256(identityClaim!);


        async Task<Result<(UserContext UserContext, string IdentityHash)>> GetUserContext(string identityHash)
        {
            var (_, isFailure, userContext, error) = await _userManagementClient.Get(identityClaim!, cancellationToken);
            return isFailure
                ? Result.Failure<(UserContext UserContext, string IdentityHash)>(error)
                : (userContext, identityHash);
        }


        async Task<Result<MemberResponse>> AddMember((UserContext UserContext, string IdentityHash) tuple)
        {
            var (userContext, identityHash) = tuple;

            var email = userContext.Email;
            if (email is null)
                return Result.Failure<MemberResponse>("An email is required for member creation. Please, check your security claims.");

            var member = await _context.Members
                .SingleOrDefaultAsync(m => m.Email == email, cancellationToken);
            _context.DetachEntities();

            if (member is not null)
                return await AddEmployee(member.Id, identityHash, cancellationToken);

            return await AddManager(userContext, identityHash, cancellationToken);
        }
    }


    public Task<Result<MemberResponse>> Deactivate(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(DeactivateInternal)
            .Bind(memberId => GetMember(memberId, cancellationToken));


        Result Validate()
        {
            var validator = new MemberRequestValidator(memberContext, _context);
            return validator.ValidateChangeState(request).ToResult();
        }


        Task<Result<int>> DeactivateInternal()
            => ChangeState(request.Id!.Value, false, cancellationToken);
    }


    public async Task<List<MemberResponse>> Get(int accountId, CancellationToken cancellationToken = default)
    {
        var members = await _context.Members
            .Where(m => m.AccountId == accountId)
            .Select(MemberProjection())
            .ToListAsync(cancellationToken);

        var memberAccounts = members
            .Select(m => m.Id);

        var invitationStates = await _invitationService.GetState(memberAccounts, cancellationToken);

        var result = new List<MemberResponse>(members.Count);
        result.AddRange(members.Select(member => invitationStates.TryGetValue(member.Id, out var state)
            ? new MemberResponse(member, state)
            : member));

        return result;
    }


    public Task<Result<MemberResponse>> GetCurrent(MemberContext? memberContext, CancellationToken cancellationToken = default)
        => Result.Success()
            .Bind(async () => await GetMember(memberContext!.Id, cancellationToken))
            .Ensure(x => !x.Equals(default), "There are no members with these parameters.");


    public Task<Result<MemberResponse>> RegenerateQr(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default)
        => ValidateGeneral(memberContext, new MemberRequest(memberId, accountId))
            .Bind(() => AssignMemberCode(memberId, cancellationToken))
            .Bind(_ => GetMember(memberId, cancellationToken));


    public Task<Result> Remove(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default)
    {
        return Validate()
            .BindWithTransaction(_context, () => RemoveInvitation()
                .Bind(RemoveMember));


        Result Validate()
        {
            var validator = new MemberRequestValidator(memberContext, _context);
            return validator.ValidateRemove(new MemberRequest(memberId, accountId)).ToResult();
        }


        async Task<Result> RemoveInvitation()
        {
            await _invitationService.Revoke(memberId, cancellationToken);
            return Result.Success();
        }


        async Task<Result> RemoveMember()
        {
            var member = await _context.Members
                .SingleAsync(m => m.Id == memberId, cancellationToken);

            if (member.Permissions == MemberPermissions.Manager)
                return Result.Failure("You can't remove a member with Manager permissions.");

            _context.Members.Remove(member);
            await _context.SaveChangesAsync(cancellationToken);

            // var (_, isFailure, error) = await _stripeAccountService.Remove(memberId, cancellationToken);
            // if (isFailure)
            //     return Result.Failure(error);

            return Result.Success();
        }
    }


    public Task<Result<MemberResponse>> Update(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
    {
        return ValidateGeneral(memberContext, request)
            .Bind(UpdateMember)
            .Bind(() => GetMember(request.Id!.Value, cancellationToken));


        async Task<Result> UpdateMember()
        {
            var targetMember = await _context.Members
                .SingleOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (targetMember is null)
                return Result.Failure($"The member with ID {request.Id} was not found.");

            targetMember.Email = request.Email;
            targetMember.FirstName = request.FirstName;
            targetMember.LastName = request.LastName;
            targetMember.Permissions = request.Permissions;
            targetMember.Position = request.Position;

            targetMember.Modified = DateTime.UtcNow;

            _context.Members.Update(targetMember);
            await _context.SaveChangesAsync(cancellationToken);

            // var (_, isFailure, error) = await _stripeAccountService.Update(request, cancellationToken);
            // if (isFailure)
            //     return Result.Failure(error);

            return Result.Success();
        }
    }


    private Result ValidateGeneral(MemberContext memberContext, MemberRequest request)
    {
        var validator = new MemberRequestValidator(memberContext, _context);
        return validator.ValidateGeneral(request).ToResult();
    }


    private Task<Result<MemberResponse>> AddEmployee(int memberId, string identityHash, CancellationToken cancellationToken)
    {
        return Result.Success()
            .BindWithTransaction(_context, () => UpdateHash()
                .Map(() => _invitationService.Redeem(memberId, cancellationToken))
                .Bind(_ => AssignMemberCode(memberId, cancellationToken))
                .Bind(_ => GetMember(memberId, cancellationToken)));


        async Task<Result> UpdateHash()
        {
            var member = await _context.Members
                .SingleAsync(m => m.Id == memberId, cancellationToken);

            member.IdentityHash = identityHash;
            _context.Members.Update(member);

            await _context.SaveChangesAsync(cancellationToken);
            _context.DetachEntities();

            return Result.Success();
        }
    }


    private Task<Result<MemberResponse>> AddManager(UserContext userContext, string identityHash, CancellationToken cancellationToken)
    {
        return Result.Success()
            .BindWithTransaction(_context, () => AddMember()
                .Bind(memberId => AssignMemberCode(memberId, cancellationToken))
                .Bind(memberId => GetMember(memberId, cancellationToken)));


        Task<Result<int>> AddMember()
            => AddMemberInternal(identityHash, null, userContext.GivenName!, userContext.Surname!, MemberPermissions.Manager, userContext.Email!, null,
                cancellationToken);
    }


    private async Task<Result<int>> AddMemberInternal(string identityHash, int? accountId, string firstName, string lastName, MemberPermissions permissions,
        string? email, string? position, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var newMember = new Member
        {
            AccountId = accountId,
            Created = now,
            Email = email,
            IdentityHash = identityHash,
            FacilityId = await GetFacilityId(),
            FirstName = firstName,
            IsActive = true,
            LastName = lastName,
            MemberCode = string.Empty,
            Modified = now,
            Permissions = permissions,
            Position = position,
            QrCodeUrl = string.Empty
        };

        _context.Members.Add(newMember);
        await _context.SaveChangesAsync(cancellationToken);
        _context.DetachEntities();

        // var (_, isFailure, error) = await _stripeAccountService
        //     .Add(new MemberRequest(newMember.Id, accountId, firstName, lastName, email, permissions, position), cancellationToken);
        // if (isFailure)
        //     return Result.Failure<int>(error);

        return newMember.Id;


        async Task<int?> GetFacilityId()
        {
            if (accountId is null)
                return null;

            return await _context.Facilities
                .Where(f => f.AccountId == accountId && f.IsDefault)
                .Select(f => f.Id)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }


    private async Task<Result<int>> AssignMemberCode(int memberId, CancellationToken cancellationToken)
    {
        var memberCode = MemberCodeGenerator.Compute(memberId);

        var (_, isFailure, qrCodeUrl, error) = await _qrCodeGenerator
            .Generate(memberCode, cancellationToken);

        if (isFailure)
        {
            _logger.LogAmazonS3Unreachable(error);
            qrCodeUrl = string.Empty;
        }

        var member = await _context.Members
            .Where(m => m.Id == memberId)
            .SingleOrDefaultAsync(cancellationToken);

        if (member is null)
            return Result.Failure<int>("The member was not found.");

        member.MemberCode = memberCode;
        member.QrCodeUrl = qrCodeUrl;

        _context.Members.Update(member);
        await _context.SaveChangesAsync(cancellationToken);

        return memberId;
    }


    private async Task<Result<int>> ChangeState(int memberId, bool state, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .Where(m => m.Id == memberId)
            .FirstOrDefaultAsync(cancellationToken);

        if (member is null)
            return Result.Failure<int>($"No members found with ID {memberId}");

        member.IsActive = state;
        member.Modified = DateTime.UtcNow;

        _context.Members.Update(member);
        await _context.SaveChangesAsync(cancellationToken);
        _context.DetachEntities();

        return member.Id;
    }


    private async Task<Result<MemberResponse>> GetMember(int memberId, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .Where(m => m.Id == memberId)
            .Select(MemberProjection())
            .SingleOrDefaultAsync(cancellationToken);

        if (member.Equals(default))
            return Result.Failure<MemberResponse>($"The member with ID {memberId} was not found.");

        var state = await _invitationService.GetState(memberId, cancellationToken);
        return new MemberResponse(member, state);
    }


    private static Expression<Func<Member, MemberResponse>> MemberProjection()
        => member => new MemberResponse(member.Id, member.AccountId, member.FacilityId, member.FirstName, member.LastName, member.AvatarUrl, member.Email,
            member.Position, member.MemberCode, member.QrCodeUrl, member.Permissions, InvitationStates.None, member.IsActive);


    private readonly IStripeAccountService _stripeAccountService;
    private readonly AetherDbContext _context;
    private readonly IInvitationService _invitationService;
    private readonly ILogger<MemberService> _logger;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly IUserManagementClient _userManagementClient;
}