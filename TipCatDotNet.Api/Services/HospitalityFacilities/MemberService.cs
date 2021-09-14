using System;
using System.Linq;
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
        public MemberService(ILoggerFactory loggerFactory, AetherDbContext context, IMicrosoftGraphClient microsoftGraphClient)
        {
            _context = context;
            _microsoftGraphClient = microsoftGraphClient;
            _logger = loggerFactory.CreateLogger<MemberService>();
        }


        public async Task<Result<MemberInfoResponse>> AddCurrent(string? identityClaim, MemberPermissions permissions, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => identityClaim is not null, "The provided Jwt token contains no ID. Highly likely this is a security configuration issue.")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(CalculateHash)
                .Ensure(async (identityHash) => !(await CheckIfMemberAlreadyAdded(identityHash)), "Another user was already added from this token data.")
                .Bind(GetUserContext)
                .Ensure(tuple =>
                {
                    var (context, _) = tuple;
                    return !string.IsNullOrWhiteSpace(context.GivenName);
                }, "Can't create a member without a given name.")
                .Ensure(tuple =>
                {
                    var (context, _) = tuple;
                    return !string.IsNullOrWhiteSpace(context.Surname);
                }, "Can't create a member without a surname.")
                .BindWithTransaction(_context, async tuple => await AddMemberToDb(tuple)
                    .Bind(AssignMemberCode));


            async Task<bool> CheckIfMemberAlreadyAdded(string identityHash)
                => await _context.Members
                    .Where(m => m.IdentityHash == identityHash)
                    .AnyAsync(cancellationToken);


            Result<string> CalculateHash() 
                => HashGenerator.ComputeSha256(identityClaim!);


            async Task<Result<(User, string)>> GetUserContext(string identityHash) 
                => (await _microsoftGraphClient.GetUser(identityClaim!, cancellationToken), identityHash);


            async Task<Result<int>> AddMemberToDb((User, string) tuple)
            {
                var (userContext, identityHash) = tuple;
                var now = DateTime.UtcNow;

                var email = userContext.Identities
                    ?.Where(i => i.SignInType == EmailSignInType)
                    .FirstOrDefault()
                    ?.IssuerAssignedId;

                var newMember = new Member
                {
                    Created = now,
                    Email = email,
                    IdentityHash = identityHash,
                    FirstName = userContext.GivenName,
                    LastName = userContext.Surname,
                    MemberCode = string.Empty,
                    Modified = now,
                    Permissions = permissions,
                    QrCodeUrl = string.Empty,
                    State = ModelStates.Active
                };

                _context.Members.Add(newMember);
                await _context.SaveChangesAsync(cancellationToken);

                return newMember.Id;
            }
            

            async Task<Result<MemberInfoResponse>> AssignMemberCode(int memberId)
            {
                var memberCode = MemberCodeGenerator.Compute(memberId);
                var qrCodeUrl = $"/{memberCode}"; // TODO: add real code generation and url here

                var member = await _context.Members
                    .Where(m => m.Id == memberId)
                    .SingleOrDefaultAsync(cancellationToken);

                member.MemberCode = memberCode;
                member.QrCodeUrl = qrCodeUrl;

                await _context.SaveChangesAsync(cancellationToken);

                return new MemberInfoResponse(member.Id, member.FirstName, member.LastName, member.Email, member.Permissions);
            }
        }


        public async Task<Result<MemberInfoResponse>> GetCurrent(MemberContext? memberContext, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Bind(async () => await GetInfo())
                .Ensure(x => !x.Equals(default), "There is no members with these parameters.");


            async Task<Result<MemberInfoResponse>> GetInfo()
                => await _context.Members
                    .Where(m => m.Id == memberContext!.Id)
                    .Select(m => new MemberInfoResponse(m.Id, m.FirstName, m.LastName, m.Email, m.Permissions))
                    .SingleOrDefaultAsync(cancellationToken);
        }

        
        public async Task<Result<MemberAvatarResponse>> UpdateAvatar(string? id, MemberAvatarRequest request, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => id is not null, "The provided Jwt token contains no ID. Highly likely this is a security configuration issue.")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(UpdateAvatarInDb);
            
            async Task<Result<MemberAvatarResponse>> UpdateAvatarInDb()
            {
                // TODO add file handler
                var newAvatarUrl = "";
                var identityHash = HashGenerator.ComputeSha256(id!);
                var member = await _context.Members
                    .Where(m => m.IdentityHash == identityHash)
                    .SingleOrDefaultAsync(cancellationToken);

                member.AvatarUrl = newAvatarUrl;
                
                await _context.SaveChangesAsync(cancellationToken);

                return new MemberAvatarResponse("member.Id, member.FirstName, member.LastName, member.Email, member.Permissions");
            }
        }
        
        
        public async Task<Result<MemberInfoResponse>> UpdateCurrent(string? id, MemberUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => id is not null, "The provided Jwt token contains no ID. Highly likely this is a security configuration issue.")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(UpdateMemberInDb);
            
            async Task<Result<MemberInfoResponse>> UpdateMemberInDb()
            {
                var identityHash = HashGenerator.ComputeSha256(id!);
                var member = await _context.Members
                    .Where(m => m.IdentityHash == identityHash)
                    .SingleOrDefaultAsync(cancellationToken);

                member.LastName = request.LastName;
                member.FirstName = request.FirstName;
                member.Email = request.Email;
                
                await _context.SaveChangesAsync(cancellationToken);

                return new MemberInfoResponse(member.Id, member.FirstName, member.LastName, member.Email, member.Permissions);
            }
        }
        

        public const string EmailSignInType = "emailAddress";

        private readonly AetherDbContext _context;
        private readonly ILogger<MemberService> _logger;
        private readonly IMicrosoftGraphClient _microsoftGraphClient;
    }
}
