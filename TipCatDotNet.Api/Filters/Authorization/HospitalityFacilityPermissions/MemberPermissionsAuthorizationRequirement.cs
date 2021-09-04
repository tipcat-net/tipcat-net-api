using Microsoft.AspNetCore.Authorization;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions

{
    public class MemberPermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public MemberPermissionsAuthorizationRequirement(MemberPermissions permissions)
        {
            Permissions = permissions;
        }


        public MemberPermissions Permissions { get; }
    }
}
