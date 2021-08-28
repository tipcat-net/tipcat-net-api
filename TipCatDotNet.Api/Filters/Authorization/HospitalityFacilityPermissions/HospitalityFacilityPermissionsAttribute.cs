using Microsoft.AspNetCore.Authorization;

namespace TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions
{
    public class HospitalityFacilityPermissionsAttribute : AuthorizeAttribute
    {
        public HospitalityFacilityPermissionsAttribute(Models.HospitalityFacilities.Enums.HospitalityFacilityPermissions permissions)
        {
            Policy = string.Concat(PolicyPrefix, permissions);
        }


        public const string PolicyPrefix = "ServiceProviderPermissions_";
    }
}
