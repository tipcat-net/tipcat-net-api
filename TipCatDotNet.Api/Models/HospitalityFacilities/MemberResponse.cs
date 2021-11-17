using System;
using System.ComponentModel.DataAnnotations;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.Permissions.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberResponse
    {
        public MemberResponse(int id, int? accountId, int? facilityId, string firstName, string lastName, string? email, string memberCode, string? qrCodeUrl,
            MemberPermissions permissions, InvitationStates invitationState)
        {
            Id = id;
            AccountId = accountId;
            Email = email;
            FacilityId = facilityId;
            FirstName = firstName;
            InvitationState = invitationState;
            LastName = lastName;
            MemberCode = memberCode;
            QrCodeUrl = qrCodeUrl;
            Permissions = permissions;
        }


        public MemberResponse(in MemberResponse response, InvitationStates state) : this(response.Id, response.AccountId, response.FacilityId,
            response.FirstName, response.LastName, response.Email, response.MemberCode, response.QrCodeUrl, response.Permissions, state)
        { }


        public override bool Equals(object? obj) => obj is MemberResponse other && Equals(other);


        public bool Equals(in MemberResponse other)
            => (Id, FirstName, LastName, Email, Permissions, InvitationState, FacilityId)
                == (other.Id, other.FirstName, other.LastName, other.Email, other.Permissions, other.InvitationState, other.FacilityId);


        public override int GetHashCode() => HashCode.Combine(Id, FirstName, LastName, Email, (int)Permissions, (int)InvitationState, FacilityId);


        public static bool operator ==(MemberResponse left, MemberResponse right) => left.Equals(right);


        public static bool operator !=(MemberResponse left, MemberResponse right) => !(left == right);


        [Required]
        public int Id { get; }

        public int? AccountId { get; }
        
        public string? Email { get; }

        public int? FacilityId { get; }

        [Required]
        public string FirstName { get; }

        [Required]
        public InvitationStates InvitationState { get; }

        [Required]
        public string LastName { get; }

        [Required]
        public string MemberCode { get; }

        public string? QrCodeUrl { get; }

        [Required]
        public MemberPermissions Permissions { get; }
    }
}