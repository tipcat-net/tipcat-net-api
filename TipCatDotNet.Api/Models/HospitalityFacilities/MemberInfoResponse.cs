using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberInfoResponse
    {
        public MemberInfoResponse(string name, string lastName, string email, MemberPermissions permissions)
        {
            FirstName = name;
            LastName = lastName;
            Email = email;
            Permissions = permissions;
        }

        
        public string FirstName { get; init; }
        
        public string LastName { get; init; }
        
        public string Email { get; init; }
        
        public MemberPermissions Permissions { get; init; }
    }
}