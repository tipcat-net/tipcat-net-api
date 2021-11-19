using System.ComponentModel.DataAnnotations;
using Stripe;

namespace TipCatDotNet.Api.Models.Payments
{
    public readonly struct PaymentDetailsResponse
    {
        public PaymentDetailsResponse(MemberInfo member)
        {
            Member = member;
            ClientSecret = null;
            PaymentIntentId = null;
        }


        public PaymentDetailsResponse(MemberInfo member, PaymentIntent? intent)
        {
            Member = member;
            ClientSecret = (intent != null) ? intent.ClientSecret : null;
            PaymentIntentId = (intent != null) ? intent.Id : null;
        }


        public override bool Equals(object? obj)
            => obj is PaymentDetailsResponse other && Equals(in other);


        public bool Equals(in PaymentDetailsResponse other)
            => Member.Equals(other.Member);


        public override int GetHashCode()
            => Member.GetHashCode();


        public static bool operator ==(PaymentDetailsResponse left, PaymentDetailsResponse right)
        {
            return left.Equals(right);
        }


        public static bool operator !=(PaymentDetailsResponse left, PaymentDetailsResponse right)
        {
            return !(left == right);
        }


        [Required]
        public MemberInfo Member { get; }
        public string? ClientSecret { get; }
        public string? PaymentIntentId { get; }



        public readonly struct MemberInfo
        {
            public MemberInfo(int id, string firstName, string lastName, string? avatarUrl)
            {
                Id = id;
                FirstName = firstName;
                LastName = lastName;
                AvatarUrl = avatarUrl;
            }


            public override bool Equals(object? obj)
                => obj is MemberInfo other && Equals(in other);


            public bool Equals(in MemberInfo other)
                => (Id, FirstName, LastName, AvatarUrl) == (other.Id, other.FirstName, other.LastName, other.AvatarUrl);


            public override int GetHashCode()
                => (Id, FirstName, LastName, AvatarUrl).GetHashCode();


            public static bool operator ==(MemberInfo left, MemberInfo right)
            {
                return left.Equals(right);
            }


            public static bool operator !=(MemberInfo left, MemberInfo right)
            {
                return !(left == right);
            }


            [Required]
            public int Id { get; }
            [Required]
            public string FirstName { get; }
            [Required]
            public string LastName { get; }
            public string? AvatarUrl { get; }
        }
    }
}