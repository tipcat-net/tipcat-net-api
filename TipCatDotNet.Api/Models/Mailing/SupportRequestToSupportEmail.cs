using TipCatDotNet.Api.Models.Company;

namespace TipCatDotNet.Api.Models.Mailing;

public record SupportRequestToSupportEmail(int MemberId, string MemberName, string Content, in CompanyInfo CompanyInfo) : BaseEmail(CompanyInfo);