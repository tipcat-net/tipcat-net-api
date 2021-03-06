using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Payments;

public interface IStripeAccountService
{
    Task<Result> Add(MemberRequest request, CancellationToken cancellationToken);
    Task<Result> AttachDefaultExternal(PayoutMethodRequest request, CancellationToken cancellationToken);
    Task<Result<StripeAccountResponse>> Retrieve(MemberRequest request, CancellationToken cancellationToken);
    Task<Result> Update(MemberRequest request, CancellationToken cancellationToken);
    Task<Result> Remove(int memberId, CancellationToken cancellationToken);
    // TODO : get cards info; 
}