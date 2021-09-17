using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    [Table("members")]
    public class Member
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("identity_hash")]
        [StringLength(64)]
        public string IdentityHash { get; set; } = null!;
        
        [Column("first_name")]
        [StringLength(128)]
        public string FirstName { get; set; } = null!;
        
        [Column("last_name")]
        [StringLength(128)]
        public string LastName { get; set; } = null!;
        
        [Column("email")]
        [StringLength(128)]
        public string? Email { get; set; }
        
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }
        
        [Column("member_code")]
        [StringLength(16)]
        public string MemberCode { get; set; } = null!;

        [Column("qr_code_url")]
        public string QrCodeUrl { get; set; } = null!;

        [Column("permissions")]
        public MemberPermissions Permissions { get; set; } = MemberPermissions.None;

        [Column("invitation_code")]
        public string? InvitationCode { get; set; }

        [Column("invitation_state")]
        public InvitationStates InvitationState { get; set; } = InvitationStates.None;

        [Column("state")]
        public ModelStates State { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }
                
        [Column("modified")]
        public DateTime Modified { get; set; }
    }
}