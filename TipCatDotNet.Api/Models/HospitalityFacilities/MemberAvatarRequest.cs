using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public struct MemberAvatarRequest
    {
        public MemberAvatarRequest(IFormFile avatar)
        {
            Avatar = avatar;
        }
        
        public IFormFile Avatar { get; set; }
    }
}