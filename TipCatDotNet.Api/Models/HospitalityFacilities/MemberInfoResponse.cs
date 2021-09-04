using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberInfoResponse
    {
        public MemberInfoResponse(string id, string name, string lastName, string email, MemberPermissions permissions)
        {
            Id = id;
            FirstName = name;
            LastName = lastName;
            Email = email;
            Permissions = permissions;
        }


        public string Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public MemberPermissions Permissions { get; }
    }
}