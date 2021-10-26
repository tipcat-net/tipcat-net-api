using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.Payments
{
    public readonly struct PaymentDetailsResponse
    {
        public PaymentDetailsResponse(MemberInfo member)
        {
            Member = member;
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