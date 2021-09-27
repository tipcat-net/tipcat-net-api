using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Infrastructure.FunctionalExtensions;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;
using TipCatDotNet.Api.Services.Graph;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class MemberService : IMemberService
    {
        public MemberService(ILoggerFactory loggerFactory, AetherDbContext context, IMicrosoftGraphClient microsoftGraphClient, QrCodeGenerator qrCodeGenerator)
        {
            _context = context;
            _microsoftGraphClient = microsoftGraphClient;
            _logger = loggerFactory.CreateLogger<MemberService>();
            _qrCodeGenerator = qrCodeGenerator;
        }


        public Task<Result<MemberResponse>> Add(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
        {
            return Validate()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, request.AccountId)
                .Ensure(IsAccountHasNoManager, "The target account has a manager already.")
                .BindWithTransaction(_context,
                    () => AddMember()
                        .Bind(CreateAndRegisterInvitation)
                        .Bind(memberId => GetMember(memberId, cancellationToken)));


            Result Validate()
            {
                var validator = new MemberRequestAddValidator();
                var validationResult = validator.Validate(request);
                if (validationResult.IsValid)
                    return Result.Success();

                return validationResult.ToFailureResult();

            }


            async Task<bool> IsAccountHasNoManager()
            {
                if (request.Permissions != MemberPermissions.Manager)
                    return true;

                return !await _context.Members
                    .AnyAsync(m => m.AccountId == request.AccountId && m.Permissions == MemberPermissions.Manager, cancellationToken);
            }


            Task<Result<int>> AddMember()
                => AddMemberInternal(string.Empty, request.AccountId, request.FirstName, request.LastName, request.Permissions, request.Email, cancellationToken);


            async Task<Result<int>> CreateAndRegisterInvitation(int memberId)
            {
                var invitation = await _microsoftGraphClient.InviteMember(request.Email!, cancellationToken);

                var member = await _context.Members
                    .SingleAsync(m => m.Id == memberId, cancellationToken);

                member.InvitationCode = invitation.InviteRedeemUrl;
                member.InvitationState = InvitationStates.Sent;

                _context.Members.Update(member);
                await _context.SaveChangesAsync(cancellationToken);

                return memberId;
            }
        }


        public Task<Result<MemberResponse>> AddCurrent(string? identityClaim, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Ensure(() => identityClaim is not null, "The provided Jwt token contains no ID. Highly likely this is a security configuration issue.")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(ComputeHash)
                .Ensure(async identityHash => !await CheckIfMemberAlreadyAdded(identityHash), "Another user was already added from this token data.")
                .Bind(GetUserContext)
                .Bind(AddMember);


            async Task<bool> CheckIfMemberAlreadyAdded(string identityHash)
                => await _context.Members
                    .Where(m => m.IdentityHash == identityHash)
                    .AnyAsync(cancellationToken);


            Result<string> ComputeHash()
                => HashGenerator.ComputeSha256(identityClaim!);


            async Task<Result<(User UserContext, string IdentityHash)>> GetUserContext(string identityHash)
                => (await _microsoftGraphClient.GetUser(identityClaim!, cancellationToken), identityHash);


            async Task<Result<MemberResponse>> AddMember((User UserContext, string IdentityHash) tuple)
            {
                var (userContext, identityHash) = tuple;

                var email = GetEmailFromUserContext(userContext);
                if (email is null)
                    return Result.Failure<MemberResponse>("An email is required for member creation. Please, check your security claims.");

                var member = await _context.Members
                    .SingleOrDefaultAsync(m => m.Email == email, cancellationToken);

                if (member is not null)
                    return await AddEmployee(member.Id, identityHash, cancellationToken);

                return await AddManager(userContext, identityHash, cancellationToken);
            }
        }


        public Task<Result<List<MemberResponse>>> Get(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default)
            => Result.Success()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, accountId)
                .Bind(() => GetMembers(accountId, cancellationToken));


        public Task<Result<MemberResponse>> Get(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default)
            => Result.Success()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, accountId)
                .EnsureTargetMemberBelongsToAccount(_context, memberId, accountId, cancellationToken)
                .Bind(() => GetMember(memberId, cancellationToken));


        public Task<Result<MemberResponse>> GetCurrent(MemberContext? memberContext, CancellationToken cancellationToken = default)
            => Result.Success()
                .Bind(async () => await GetMember(memberContext!.Id, cancellationToken))
                .Ensure(x => !x.Equals(default), "There is no members with these parameters.");


        public Task<Result> Remove(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Ensure(() => memberContext.Id != memberId, "You can't remove yourself.")
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, accountId)
                .EnsureTargetMemberBelongsToAccount(_context, memberId, accountId, cancellationToken)
                .Bind(RemoveMember);


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
            return Validate()
                .EnsureCurrentMemberBelongsToAccount(memberContext.AccountId, request.AccountId)
                .EnsureTargetMemberBelongsToAccount(_context, request.Id, request.AccountId, cancellationToken)
                .Bind(UpdateMember)
                .Bind(() => GetMember((int)request.Id!, cancellationToken));


            Result Validate()
            {
                var validator = new MemberRequestUpdateValidator();
                var validationResult = validator.Validate(request);
                if (!validationResult.IsValid)
                    return validationResult.ToFailureResult();

                return Result.Success();
            }


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


        private Task<Result<MemberResponse>> AddEmployee(int memberId, string identityHash, CancellationToken cancellationToken)
        {
            return Result.Success()
                .BindWithTransaction(_context, () => UpdateHash()
                    .Bind(_ => AssignMemberCode(memberId, cancellationToken))
                    .Bind(_ => GetMember(memberId, cancellationToken)));


            async Task<Result<int>> UpdateHash()
            {
                var member = await _context.Members
                    .SingleAsync(m => m.Id == memberId, cancellationToken);

                member.IdentityHash = identityHash;
                member.InvitationState = InvitationStates.Accepted;
                _context.Members.Update(member);

                await _context.SaveChangesAsync(cancellationToken);

                return member.Id;
            }
        }


        private Task<Result<MemberResponse>> AddManager(User userContext, string identityHash, CancellationToken cancellationToken)
        {
            return Result.Success()
                .Ensure(() => !string.IsNullOrWhiteSpace(userContext.GivenName), "Can't create a member without a given name.")
                .Ensure(() => !string.IsNullOrWhiteSpace(userContext.Surname), "Can't create a member without a surname.")
                .BindWithTransaction(_context, () => AddMember()
                    .Bind(memberId => AssignMemberCode(memberId, cancellationToken))
                    .Bind(memberId => GetMember(memberId, cancellationToken)));


            Task<Result<int>> AddMember()
            {
                var email = GetEmailFromUserContext(userContext);
                return AddMemberInternal(identityHash, null, userContext.GivenName, userContext.Surname, MemberPermissions.Manager, email, cancellationToken);
            }
        }


        private async Task<Result<int>> AddMemberInternal(string identityHash, int? accountId, string firstName, string lastName, MemberPermissions permissions, string? email,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var newMember = new Member
            {
                AccountId = accountId,
                Created = now,
                Email = email,
                IdentityHash = identityHash,
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
        }


        private async Task<Result<int>> AssignMemberCode(int memberId, CancellationToken cancellationToken)
        {
            var memberCode = MemberCodeGenerator.Compute(memberId);
            var qrCodeUrl = await _qrCodeGenerator.Generate(memberId, cancellationToken);

            var member = await _context.Members
                .Where(m => m.Id == memberId)
                .SingleOrDefaultAsync(cancellationToken);

            member.MemberCode = memberCode;
            member.QrCodeUrl = qrCodeUrl;

            _context.Members.Update(member);
            await _context.SaveChangesAsync(cancellationToken);

            return memberId;
        }


        private static string? GetEmailFromUserContext(User userContext)
            => userContext.Identities
                ?.Where(i => i.SignInType == EmailSignInType)
                .FirstOrDefault()
                ?.IssuerAssignedId;


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


        private static Expression<Func<Member, MemberResponse>> MemberProjection()
            => member => new MemberResponse(member.Id, member.AccountId, member.FirstName, member.LastName, member.Email, member.Permissions);


        public const string EmailSignInType = "emailAddress";

        private readonly AetherDbContext _context;
        private readonly ILogger<MemberService> _logger;
        private readonly IMicrosoftGraphClient _microsoftGraphClient;

        private readonly QrCodeGenerator _qrCodeGenerator;
    }
}
