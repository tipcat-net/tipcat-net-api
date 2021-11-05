using System;
using TipCatDotNet.Api.Models.Auth.Enums;

namespace TipCatDotNet.Api.Data.Models.Auth
{
    public class MemberInvitation
    {
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Link { get; set; }

        public int MemberId { get; set; }

        public InvitationStates State { get; set; } = InvitationStates.None;

        public DateTime Created { get; set; }
                
        public DateTime Modified { get; set; }
    }
}
