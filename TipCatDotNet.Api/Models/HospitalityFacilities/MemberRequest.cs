using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberRequest
    {
        [JsonConstructor]
        public MemberRequest(int? id, int? accountId, string firstName, string lastName, string? email, MemberPermissions permissions)
        {
            Id = id;
            AccountId = accountId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Permissions = permissions;
        }


        public MemberRequest(int? id, int? accountId)
        {
            Id = id;
            AccountId = accountId;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = null;
            Permissions = MemberPermissions.None;
        }


        public MemberRequest(int? id, in MemberRequest request) : this(id, request.AccountId, request.FirstName, request.LastName, request.Email,
            request.Permissions)
        { }


        public MemberRequest(int? id, int? accountId, in MemberRequest request) : this(id, accountId, request.FirstName, request.LastName, request.Email,
            request.Permissions)
        { }


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
        public MemberPermissions Permissions { get; }
    }
}