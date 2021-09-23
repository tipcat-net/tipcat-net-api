using System;
using System.ComponentModel.DataAnnotations;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    public class Member
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string IdentityHash { get; set; } = null!;

        public int? AccountId { get; set; }
        
        [StringLength(128)]
        public string FirstName { get; set; } = null!;
        
        [StringLength(128)]
        public string LastName { get; set; } = null!;
        
        [StringLength(128)]
        public string? Email { get; set; }
        
        public string? AvatarUrl { get; set; }
        
        [StringLength(16)]
        public string MemberCode { get; set; } = null!;

        public string QrCodeUrl { get; set; } = null!;

        public MemberPermissions Permissions { get; set; } = MemberPermissions.None;

        public string? InvitationCode { get; set; }

        public InvitationStates InvitationState { get; set; } = InvitationStates.None;

        public ModelStates State { get; set; }

        public DateTime Created { get; set; }
                
        public DateTime Modified { get; set; }
    }
}