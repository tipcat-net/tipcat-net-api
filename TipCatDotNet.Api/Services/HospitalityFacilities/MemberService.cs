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
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class MemberService : IMemberService
    {
        public MemberService(ILoggerFactory loggerFactory, AetherDbContext context, GraphServiceClient graphServiceClient)
        {
            _context = context;
            _graphServiceClient = graphServiceClient;
            _logger = loggerFactory.CreateLogger<MemberService>();
        }


        public async Task<Result<MemberInfoResponse>> Add(string? id, MemberPermissions permissions, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => id is not null, "The provided Jwt token contains no ID. Highly likely this is a security configuration issue.")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(GetUserContext)
                .Ensure(context => !string.IsNullOrWhiteSpace(context.GivenName), "Can't create a member without a given name.")
                .Ensure(context => !string.IsNullOrWhiteSpace(context.Surname), "Can't create a member without a surname.")
                //.BindWithTransactionScope(async context => await AddMemberToDb(context)
                //    .Bind(AssignMemberCode))
                .Bind(GenerateInfoQuickWay);


            async Task<Result<User>> GetUserContext()
                => await _graphServiceClient.Users[id]
                    .Request()
                    .Select(u => new { u.GivenName, u.Surname, u.Identities })
                    .GetAsync(cancellationToken);


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


            Result<MemberInfoResponse> GenerateInfoQuickWay(User userContext)
            {
                var email = userContext.Identities
                    ?.Where(i => i.SignInType == EmailSignInType)
                    .FirstOrDefault()
                    ?.IssuerAssignedId;

                return new MemberInfoResponse(1, userContext.GivenName, userContext.Surname, email, permissions);
            }
        }


        public const string EmailSignInType = "emailAddress";

        private readonly AetherDbContext _context;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<MemberService> _logger;
    }
}
