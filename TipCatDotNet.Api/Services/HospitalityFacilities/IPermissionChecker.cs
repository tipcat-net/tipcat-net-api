using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IPermissionChecker
    {
        public ValueTask<Result> CheckMemberPermissions(MemberContext member, MemberPermissions permissions, CancellationToken cancellationToken = default);
    }
}
