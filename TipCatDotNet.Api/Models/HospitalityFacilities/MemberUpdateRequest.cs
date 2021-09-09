using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public  struct MemberUpdateRequest
    {

        /*public MemberUpdateRequest([Required][NotNull]string firstName, [Required][NotNull]string lastName, [EmailAddress]string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }*/
        
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
    }
}