using TipCatDotNet.Api.Models.Company;

namespace TipCatDotNet.Api.Models.Mailing;

public record MemberInvitationEmail : BaseEmail
{
    public MemberInvitationEmail(string accountName, string link, in CompanyInfo companyInfo) : base(in companyInfo)
    {
        AccountName = accountName;
        Link = link;
    }


    public string AccountName { get; }
    public string Link { get; }
}