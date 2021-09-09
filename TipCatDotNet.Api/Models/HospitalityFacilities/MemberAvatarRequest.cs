using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class MemberAvatarRequest
    {
       
        public IFormFile Avatar { get; set; }
    }
}