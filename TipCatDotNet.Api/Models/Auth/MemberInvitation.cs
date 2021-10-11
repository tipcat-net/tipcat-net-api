namespace TipCatDotNet.Api.Models.Auth
{
    public readonly struct MemberInvitation
    {
        public MemberInvitation(string link, string code)
        {
            Code = code;
            Link = link;
        }


        public string Code { get; }
        public string Link { get; }
    }
}
