using System.ComponentModel.DataAnnotations;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberRequest
    {
        public MemberRequest(int? id, int? accountId, string firstName, string lastName, string? email, MemberPermissions permission)
        {
            Id = id;
            AccountId = accountId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Permission = permission;
        }


        public MemberRequest(int? id, int? accountId, in MemberRequest request)
        {
            Id = id;
            AccountId = accountId;
            Email = request.Email;
            FirstName = request.FirstName;
            LastName = request.LastName;
            Permission = request.Permission;
        }


        [Required]
        public int? Id { get; }
        [Required]
        public int? AccountId { get; }
        public string? Email { get; }
        [Required]
        public string FirstName { get; }
        [Required]
        public string LastName { get; }
        [Required]
        public MemberPermissions Permission { get; }
    }
}