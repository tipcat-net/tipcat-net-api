using System.ComponentModel.DataAnnotations;
using Stripe;

namespace TipCatDotNet.Api.Models.Payments
{
    public readonly struct PaymentDetailsResponse
    {
        public PaymentDetailsResponse(in MemberInfo member, in ProFormaInvoice proFormaInvoice, PaymentIntent? intent)
        {
            Member = member;
            ClientSecret = intent?.ClientSecret;
            PaymentIntentId = intent?.Id;
            ProFormaInvoice = proFormaInvoice;
        }


        [Required]
        public MemberInfo Member { get; }
        public string? ClientSecret { get; }
        public string? PaymentIntentId { get; }
        public ProFormaInvoice ProFormaInvoice { get; }

        
        public readonly struct MemberInfo
        {
            public MemberInfo(int id, string firstName, string lastName, string? position, string? avatarUrl, string accountName, string? facilityName)
            {
                Id = id;
                AccountName = accountName;
                AvatarUrl = avatarUrl;
                FacilityName = facilityName;
                FirstName = firstName;
                LastName = lastName;
                Position = position;
            }


            public override bool Equals(object? obj)
                => obj is MemberInfo other && Equals(in other);


            public bool Equals(in MemberInfo other)
                => (Id, FirstName, LastName, AvatarUrl) == (other.Id, other.FirstName, other.LastName, other.AvatarUrl);


            public override int GetHashCode()
                => (Id, FirstName, LastName, AvatarUrl).GetHashCode();


            public static bool operator ==(in MemberInfo left, in MemberInfo right) 
                => left.Equals(right);


            public static bool operator !=(in MemberInfo left, in MemberInfo right) 
                => !(left == right);


            [Required]
            public int Id { get; }
            [Required]
            public string AccountName { get; }
            public string? AvatarUrl { get; }
            public string? FacilityName { get; }
            [Required]
            public string FirstName { get; }
            [Required]
            public string LastName { get; }
            public string? Position { get; }
        }
    }
}