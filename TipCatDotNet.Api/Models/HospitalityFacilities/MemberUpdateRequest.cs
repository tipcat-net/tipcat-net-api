using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberUpdateRequest
    {

        public MemberUpdateRequest(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
        
        
        [Required]
        public string FirstName { get;  }
        [Required]
        public string LastName { get;}
        public string? Email { get; }
    }
}