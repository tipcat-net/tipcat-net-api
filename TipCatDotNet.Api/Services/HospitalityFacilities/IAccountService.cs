using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IAccountService
    {
        Task<Result<AccountResponse>> Add(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default);
        
        Task<Result<AccountResponse>> Get(MemberContext context, int accountId, CancellationToken cancellationToken = default);

        Task<Result<AccountResponse>> Update(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default);
    }
}
