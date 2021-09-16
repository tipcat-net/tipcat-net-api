using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberRequest
    {
        public MemberRequest(int? id, int? accountId, string firstName, string lastName, string? email)
        {
            Id = id;
            AccountId = accountId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
        }


        public MemberRequest(int? id, int? accountId, in MemberRequest request)
        {
            Id = id;
            AccountId = accountId;
            Email = request.Email;
            FirstName = request.FirstName;
            LastName = request.LastName;
        }


        [Required]
        public int? Id { get; }
        [Required]
        public int? AccountId { get; }
        [Required]
        public string FirstName { get; }
        [Required]
        public string LastName { get; }
        public string? Email { get; }
    }
}