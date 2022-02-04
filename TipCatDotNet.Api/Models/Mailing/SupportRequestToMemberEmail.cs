using TipCatDotNet.Api.Models.Company;

namespace TipCatDotNet.Api.Models.Mailing;

public record SupportRequestToMemberEmail(string MemberFirstName, string Content, in CompanyInfo CompanyInfo) : BaseEmail(CompanyInfo);