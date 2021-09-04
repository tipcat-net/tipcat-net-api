using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class MemberService : IMemberService
    {
        public MemberService(ILoggerFactory loggerFactory, GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
            _logger = loggerFactory.CreateLogger<MemberService>();
        }


        public async Task<Result<MemberInfoResponse>> Add(string? id)
        {
            return await Result.Success()
                .Ensure(() => id is not null, "The provided Jwt token contains no ID. Highly likely this is a security configuration issue.")
                .OnFailure(() => _logger.LogNoIdentifierOnMemberAddition())
                .Bind(GetUserContext)
                .Bind(BuildMemberInfo);


            async Task<Result<User>> GetUserContext()
                => await _graphServiceClient.Users[id]
                    .Request()
                    //.Select(u => new { u.GivenName, u.Surname, u.UserPrincipalName })
                    .GetAsync();


            Result<MemberInfoResponse> BuildMemberInfo(User userContext)
                => new MemberInfoResponse(id!, userContext.GivenName, userContext.Surname, userContext.UserPrincipalName, MemberPermissions.Manager);
        }


        private readonly GraphServiceClient _graphServiceClient;
        private readonly ILogger<MemberService> _logger;
    }
}
