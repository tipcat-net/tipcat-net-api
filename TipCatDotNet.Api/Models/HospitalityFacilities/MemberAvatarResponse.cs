using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct MemberAvatarResponse
    {
        public MemberAvatarResponse(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
        }
        
        [Required]
        public string AvatarUrl { get; }
    }
}