using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public  struct MemberAvatarRequest
    {
        
        public IFormFile Avatar { get; set; }
    }
}