using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.Payments
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
    }
}