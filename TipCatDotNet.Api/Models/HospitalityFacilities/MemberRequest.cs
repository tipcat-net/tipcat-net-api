using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TipCatDotNet.Api.Models.Permissions.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberRequest
    {
        [JsonConstructor]
        public MemberRequest(int? id, int? accountId, string firstName, string lastName, string? email, MemberPermissions permissions, string? position = null)
        {
            Id = id;
            AccountId = accountId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Permissions = permissions;
            Position = position;
        }


        public MemberRequest(int? id, int? accountId) : this(id, accountId, string.Empty, string.Empty, null, MemberPermissions.None)
        { }


        public MemberRequest(int? id, int? accountId, string email) : this(id, accountId, string.Empty, string.Empty, email, MemberPermissions.None)
        { }


        public MemberRequest(int? id, in MemberRequest request) : this(id, request.AccountId, request.FirstName, request.LastName, request.Email,
            request.Permissions, request.Position)
        { }


        public MemberRequest(int? id, int? accountId, in MemberRequest request) : this(id, accountId, request.FirstName, request.LastName, request.Email,
            request.Permissions, request.Position)
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
        public string? Position { get; }
    }
}