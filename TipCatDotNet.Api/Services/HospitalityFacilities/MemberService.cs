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
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;
using TipCatDotNet.Api.Models.Permissions.Enums;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class MemberService : IMemberService
    {
        public MemberService(ILoggerFactory loggerFactory, AetherDbContext context, IUserManagementClient userManagementClient,
            IQrCodeGenerator qrCodeGenerator, IInvitationService invitationService, IFacilityService facilityService)
        {
            _context = context;
            _facilityService = facilityService;
            _invitationService = invitationService;
            _logger = loggerFactory.CreateLogger<MemberService>();
            _qrCodeGenerator = qrCodeGenerator;
            _userManagementClient = userManagementClient;
        }


        public Task<Result<MemberResponse>> Add(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
        {
            return Validate()
                .BindWithTransaction(_context,
                    () => AddMemberInternal(string.Empty, request.AccountId, request.FirstName, request.LastName, request.Permissions, request.Email,
                            cancellationToken)
                        .Bind(SendInvitation)
                        .Bind(memberId => GetMember(memberId, cancellationToken)));


            Result Validate()
            {
                var validator = new MemberRequestValidator(memberContext, _context);
                return validator.ValidateAdd(request).ToResult();
            }


            async Task<Result<int>> SendInvitation(int memberId)
            {
                var modifiedRequest = new MemberRequest(memberId, in request);
                var (_, isFailure, error) = await _invitationService.Send(modifiedRequest, cancellationToken);
                return isFailure
                    ? Result.Failure<int>(error)
                    : memberId;
            }
        }


        public Task<Result<MemberResponse>> TransferToFacility(MemberContext memberContext, int facilityId, int memberId, int accountId,
            CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(() => _facilityService.TransferMember(memberId, facilityId, cancellationToken))
                .Bind(_ => GetMember(memberId, cancellationToken));


            Result Validate()
            {
                var validator = new MemberTransferValidator(memberContext);
                return validator.Validate((facilityId, memberId, accountId)).ToResult();
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


        public Task<Result<List<MemberResponse>>> Get(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default)
            => ValidateGeneral(memberContext, new MemberRequest(null, accountId))
                .Bind(() => GetMembers(accountId, cancellationToken));


        public Task<Result<MemberResponse>> Get(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default)
            => ValidateGeneral(memberContext, new MemberRequest(memberId, accountId))
                .Bind(() => GetMember(memberId, cancellationToken));


        public Task<Result<MemberResponse>> GetCurrent(MemberContext? memberContext, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(async () => await GetMember(memberContext!.Id, cancellationToken))
                .Ensure(x => !x.Equals(default), "There are no members with these parameters.");


        public Task<Result<List<MemberResponse>>> GetByFacility(MemberContext memberContext, int accountId, int facilityId, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Ensure(() => memberContext.AccountId == accountId, "The current member does not belong to the target account.")
                .Ensure(IsTargetFacilityBelongsToAccount, "The target member does not belong to the target account.")
                .Bind(() => GetMembersByFacility(accountId, facilityId, cancellationToken));


            async Task<bool> IsTargetFacilityBelongsToAccount()
                => !await _context.Facilities
                    .AnyAsync(f => f.Id == facilityId && f.AccountId == accountId, cancellationToken);
        }


        public Task<Result<MemberResponse>> RegenerateQr(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default)
            => ValidateGeneral(memberContext, new MemberRequest(memberId, accountId))
                .Bind(() => AssignMemberCode(memberId, cancellationToken))
                .Bind(_ => GetMember(memberId, cancellationToken));


        public Task<Result> Remove(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(RemoveMember);


            Result Validate()
            {
                var validator = new MemberRequestValidator(memberContext, _context);
                return validator.ValidateRemove(new MemberRequest(memberId, accountId)).ToResult();
            }
            

            async Task<Result> RemoveMember()
            {
                var member = await _context.Members
                    .SingleAsync(m => m.Id == memberId, cancellationToken);

                if (member.Permissions == MemberPermissions.Manager)
                    return Result.Failure("You can't remove a member with Manager permissions.");

                _context.Members.Remove(member);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        public Task<Result<MemberResponse>> Update(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
        {
            return ValidateGeneral(memberContext, request)
                .Bind(UpdateMember)
                .Bind(() => GetMember((int)request.Id!, cancellationToken));


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

                targetMember.Modified = DateTime.UtcNow;

                _context.Members.Update(targetMember);
                await _context.SaveChangesAsync(cancellationToken);

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
                => AddMemberInternal(identityHash, null, userContext.GivenName!, userContext.Surname!, MemberPermissions.Manager, userContext.Email!,
                    cancellationToken);
        }


        private async Task<Result<int>> AddMemberInternal(string identityHash, int? accountId, string firstName, string lastName, MemberPermissions permissions,
            string? email, CancellationToken cancellationToken)
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
                LastName = lastName,
                MemberCode = string.Empty,
                Modified = now,
                Permissions = permissions,
                QrCodeUrl = string.Empty,
                State = ModelStates.Active
            };

            _context.Members.Add(newMember);
            await _context.SaveChangesAsync(cancellationToken);
            _context.DetachEntities();

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

            member.MemberCode = memberCode;
            member.QrCodeUrl = qrCodeUrl;

            _context.Members.Update(member);
            await _context.SaveChangesAsync(cancellationToken);

            return memberId;
        }


        private async Task<Result<MemberResponse>> GetMember(int memberId, CancellationToken cancellationToken)
        {
            var member = await _context.Members
                .Where(m => m.Id == memberId)
                .Select(MemberProjection())
                .SingleOrDefaultAsync(cancellationToken);

            if (!member.Equals(default))
                return member;

            return Result.Failure<MemberResponse>($"The member with ID {memberId} was not found.");
        }


        private async Task<Result<List<MemberResponse>>> GetMembers(int accountId, CancellationToken cancellationToken)
            => await _context.Members
                .Where(m => m.AccountId == accountId)
                .Select(MemberProjection())
                .ToListAsync(cancellationToken);


        private async Task<Result<List<MemberResponse>>> GetMembersByFacility(int accountId, int facilityId, CancellationToken cancellationToken)
            => await _context.Members
                .Where(m => m.AccountId == accountId && m.FacilityId == facilityId)
                .Select(MemberProjection())
                .ToListAsync(cancellationToken);


        private static Expression<Func<Member, MemberResponse>> MemberProjection()
            => member => new MemberResponse(member.Id, member.AccountId, member.FirstName, member.LastName, member.Email, member.MemberCode, member.QrCodeUrl,
                member.Permissions);


        private readonly AetherDbContext _context;
        private readonly IFacilityService _facilityService;
        private readonly IInvitationService _invitationService;
        private readonly ILogger<MemberService> _logger;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly IUserManagementClient _userManagementClient;
    }
}
