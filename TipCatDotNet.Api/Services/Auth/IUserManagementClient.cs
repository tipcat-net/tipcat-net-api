using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Auth
{
    public interface IUserManagementClient
    {
        Task<Result<string>> Add(MemberRequest request, CancellationToken cancellationToken);

        Task<Result> ChangePassword(string email, CancellationToken cancellationToken);

        Task<Result<UserContext>> Get(string identityClaim, CancellationToken cancellationToken);
    }
}
