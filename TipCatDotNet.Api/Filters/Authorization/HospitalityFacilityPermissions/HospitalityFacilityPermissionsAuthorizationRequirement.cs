using Microsoft.AspNetCore.Authorization;

namespace TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions

{
    public class HospitalityFacilityPermissionsAuthorizationRequirement : IAuthorizationRequirement
    {
        public HospitalityFacilityPermissionsAuthorizationRequirement(Models.HospitalityFacilities.Enums.HospitalityFacilityPermissions permissions)
        {
            Permissions = permissions;
        }


        public Models.HospitalityFacilities.Enums.HospitalityFacilityPermissions Permissions { get; }
    }
}
