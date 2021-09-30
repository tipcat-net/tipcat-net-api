using System;
using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct ReceiverResponse
    {
        public ReceiverResponse(string memberCode)
        {
            MemberCode = memberCode;
        }


        [Required]
        public string MemberCode { get; }


        public override bool Equals(object? obj) => obj is MemberResponse other && Equals(other);


        public bool Equals(in MemberResponse other)
            => (MemberCode) == (other.MemberCode);


        public override int GetHashCode()
            => HashCode.Combine(MemberCode);
    }
}