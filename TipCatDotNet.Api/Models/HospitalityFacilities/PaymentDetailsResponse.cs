using System;
using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct PaymentDetailsResponse
    {
        public PaymentDetailsResponse(int memberId, string memberFirstName, string memberLastName, string? memberAvatarUrl)
        {
            MemberId = memberId;
            MemberFirstName = memberFirstName;
            MemberLastName = memberLastName;
            MemberAvatarUrl = memberAvatarUrl;
        }


        [Required]
        public int MemberId { get; }
        [Required]
        public string MemberFirstName { get; }
        [Required]
        public string MemberLastName { get; }
        public string? MemberAvatarUrl { get; }


        public override bool Equals(object? obj) => obj is PaymentDetailsResponse other && Equals(other);


        public bool Equals(in PaymentDetailsResponse other)
            => (MemberId, MemberFirstName, MemberLastName, MemberAvatarUrl) == (other.MemberId, other.MemberFirstName, other.MemberLastName, other.MemberAvatarUrl);


        public override int GetHashCode()
            => HashCode.Combine(MemberId, MemberFirstName, MemberLastName, MemberAvatarUrl);
    }
}