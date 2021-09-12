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


        public async Task<Result<MemberInfoResponse>> AddCurrent(string? id, MemberPermissions permissions, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => id is not null, "The provided Jwt token contains no ID. Highly likely this is a security configuration issue.")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(GetUserContext)
                .Ensure(context => !string.IsNullOrWhiteSpace(context.GivenName), "Can't create a member without a given name.")
                .Ensure(context => !string.IsNullOrWhiteSpace(context.Surname), "Can't create a member without a surname.")
                .BindWithTransaction(_context, async context => await AddMemberToDb(context)
                    .Bind(AssignMemberCode));


            async Task<Result<User>> GetUserContext() => await _microsoftGraphClient.GetUser(id!, cancellationToken);


            async Task<Result<int>> AddMemberToDb(User userContext)
            {
                var now = DateTime.UtcNow;
                var identityHash = HashGenerator.ComputeSha256(id!);

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
                    .Select(m => new MemberInfoResponse(m.Id, m.FirstName, m.LastName, m.Email, m.Permissions, m.AvatarUrl))
                    .SingleOrDefaultAsync(cancellationToken);
        }

        
        public async Task<Result<MemberAvatarResponse>> UpdateAvatar(int accountId, int memberId, MemberContext? memberContext, MemberAvatarRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            /*return await Result.Success()
                .Ensure(() => request.Avatar.GetSize() <= AvatarMaxSize, $"Maximum file size: {AvatarMaxSize} mb")
                .Ensure(() => request.Avatar.IsImage(), $"The uploaded file is not an image")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(UpdateAvatarInDb);
            
            async Task<Result<MemberAvatarResponse>> UpdateAvatarInDb()
            {
                var member = await _context.Members
                    .Where(m => m.Id == memberContext!.Id)
                    .SingleOrDefaultAsync(cancellationToken);
                request.Avatar.Save(AvatarPath, member.IdentityHash, out var fileName,out _);
                
                if (member.AvatarUrl != fileName)
                {
                    FileHelper.Delete(AvatarPath+member.AvatarUrl);
                    member.AvatarUrl = fileName;
                    _context.Members.Update(member);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                
                return new MemberAvatarResponse(fileName);
            }*/
        }

        public async Task<Result<MemberInfoResponse>> VerifyEmail(MemberContext memberContext, MemberVerifyEmailRequest request,
            CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => !string.IsNullOrWhiteSpace(request.Code), "The code cannot be empty")
                .Ensure(async () => await CheckCode(), "The code is error")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(UpdateMemberInDb);

            async Task<bool> CheckCode()
            {
                var requestCodeHash = EmailVerificationCodeGenerator.ToHash(request.Code);

                return await _context.Members
                    .AnyAsync(m => m.Id == memberContext!.Id && m.VerificationCodeHash == requestCodeHash, cancellationToken: cancellationToken);
            }
            
            async Task<Result<MemberInfoResponse>> UpdateMemberInDb()
            {
                var member = await _context.Members
                    .Where(m => m.Id == memberContext!.Id)
                    .SingleOrDefaultAsync(cancellationToken);

                member.LastName = member.LastNameTmp!;
                member.FirstName = member.FirstNameTmp!;
                member.Email = member.EmailTmp;
                member.FirstNameTmp = member.LastNameTmp = member.EmailTmp = null;
                _context.Members.Update(member);
                await _context.SaveChangesAsync(cancellationToken);
                return new MemberInfoResponse(member.Id, member.FirstName, member.LastName, member.Email, member.Permissions, member.AvatarUrl);
            }
        }


        public async Task<Result<MemberInfoResponse>> Update(MemberContext? memberContext, MemberUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => !string.IsNullOrEmpty(request.FirstName), "The first name cannot be empty")
                .Ensure(() => !string.IsNullOrEmpty(request.LastName), "The last name cannot be empty")
                .Ensure(() => Validations.IsEmail(request.Email!), "Invalid email")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(UpdateMemberInDb);
            
            async Task<Result<MemberInfoResponse>> UpdateMemberInDb()
            {
                var member = await _context.Members
                    .Where(m => m.Id == memberContext!.Id)
                    .SingleOrDefaultAsync(cancellationToken);

                if (member.Email != request.Email)
                {
                    member.LastNameTmp = request.LastName;
                    member.FirstNameTmp = request.FirstName;
                    member.EmailTmp = request.Email!;

                    var code = EmailVerificationCodeGenerator.Compute();
                    member.VerificationCodeHash = EmailVerificationCodeGenerator.ToHash(code);
                    
                    _context.Members.Update(member);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    SendVerificationCodeToEmail(code);

                    return new MemberInfoResponse(-1, null!, null!, null, default);
                }
                else
                {
                    member.LastName = request.LastName;
                    member.FirstName = request.FirstName;
                    
                    _context.Members.Update(member);
                    await _context.SaveChangesAsync(cancellationToken);
                    return new MemberInfoResponse(member.Id, member.FirstName, member.LastName, member.Email, member.Permissions);
                }
            }

            async Task SendVerificationCodeToEmail(string code)
            {
                // TODO add sending verification code to email
            }

            
        }
        

        public const string EmailSignInType = "emailAddress";

        // format by MB
        private const byte AvatarMaxSize = 1;
        private const string AvatarPath = "avatars/";

        private readonly AetherDbContext _context;
        private readonly ILogger<MemberService> _logger;
        private readonly IMicrosoftGraphClient _microsoftGraphClient;
    }
}
