using Microsoft.AspNetCore.Authorization;
using TipCatDotNet.Api.Models.Permissions.Enums;

namespace TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions
{
    public class MemberPermissionsAttribute : AuthorizeAttribute
    {
        public MemberPermissionsAttribute(MemberPermissions permissions)
        {
            Policy = string.Concat(PolicyPrefix, permissions);
        }


        public const string PolicyPrefix = "MemberPermissions_";
    }
}
