using System.ComponentModel.DataAnnotations;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberInfoResponse
    {
        public MemberInfoResponse(int id, string name, string lastName, string? email, MemberPermissions permissions)
        {
            Id = id;
            FirstName = name;
            LastName = lastName;
            Email = email;
            Permissions = permissions;
        }


        [Required]
        public int Id { get; }
        [Required]
        public string FirstName { get; }
        [Required]
        public string LastName { get; }
        public string? Email { get; }
        [Required]
        public MemberPermissions Permissions { get; }
    }
}