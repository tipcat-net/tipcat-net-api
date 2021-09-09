using System;
using System.ComponentModel.DataAnnotations;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberInfoResponse
    {
        public MemberInfoResponse(int id, string firstName, string lastName, string? email, MemberPermissions permissions, string? avatarUrl)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Permissions = permissions;
            AvatarUrl = avatarUrl;
        }
        
        public MemberInfoResponse(int id, string firstName, string lastName, string? email, MemberPermissions permissions)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Permissions = permissions;
            AvatarUrl = null;
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

        public string? AvatarUrl { get; } 


        public override bool Equals(object? obj) => obj is MemberInfoResponse other && Equals(other);


        public bool Equals(in MemberInfoResponse other) 
            => (Id, FirstName, LastName, Email, Permissions) == (other.Id, other.FirstName, other.LastName, other.Email, other.Permissions);


        public override int GetHashCode() => HashCode.Combine(Id, FirstName, LastName, Email, (int)Permissions);
    }
}