using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Data.Models.Auth
{
    public class MemberInvitation
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public string Link { get; set; } = null!;

        public int MemberId { get; set; }

        public InvitationStates State { get; set; } = InvitationStates.None;
    }
}
