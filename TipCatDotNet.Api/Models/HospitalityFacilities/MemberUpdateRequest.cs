using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public  struct MemberUpdateRequest
    {

        public MemberUpdateRequest(string firstName, string lastName, [EmailAddress]string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
        
        
        [Required]
        public string FirstName { get;  set;}
        [Required]
        public string LastName { get;set;}
        [EmailAddress]
        public string? Email { get; set;}
    }
}