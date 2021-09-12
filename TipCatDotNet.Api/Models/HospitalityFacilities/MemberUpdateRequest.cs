using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public  struct MemberUpdateRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
    }
}