using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public  struct MemberUpdateRequest
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set;}
        public string? Email { get; set;}
    }
}