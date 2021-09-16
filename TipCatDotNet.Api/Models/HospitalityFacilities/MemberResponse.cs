using System;
using System.ComponentModel.DataAnnotations;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberResponse
    {
        public MemberResponse(int id, int? accountId, string firstName, string lastName, string? email, MemberPermissions permissions)
        {
            Id = id;
            AccountId = accountId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Permissions = permissions;
        }


        [Required]
        public int Id { get; }
        public int? AccountId { get; }
        [Required]
        public string FirstName { get; }
        [Required]
        public string LastName { get; }
        public string? Email { get; }
        [Required]
        public MemberPermissions Permissions { get; }


        public override bool Equals(object? obj) => obj is MemberResponse other && Equals(other);


        public bool Equals(in MemberResponse other) 
            => (Id, FirstName, LastName, Email, Permissions) == (other.Id, other.FirstName, other.LastName, other.Email, other.Permissions);


        public override int GetHashCode() 
            => HashCode.Combine(Id, FirstName, LastName, Email, (int)Permissions);


        public static bool operator ==(MemberResponse left, MemberResponse right) 
            => left.Equals(right);


        public static bool operator !=(MemberResponse left, MemberResponse right) 
            => !(left == right);
    }
}