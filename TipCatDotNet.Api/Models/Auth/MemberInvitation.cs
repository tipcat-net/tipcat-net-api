namespace TipCatDotNet.Api.Models.Auth
{
    public readonly struct MemberInvitation
    {
        public MemberInvitation(string userId, string email, string connectionId)
        {
            ConnectionId = connectionId;
            Email = email;
            UserId = userId;
        }


        public string ConnectionId { get; }
        public string Email { get; }
        public string UserId { get; }
    }
}
