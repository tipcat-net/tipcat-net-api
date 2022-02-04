using TipCatDotNet.Api.Models.Company;

namespace TipCatDotNet.Api.Models.Mailing;

public abstract record BaseEmail(in CompanyInfo CompanyInfo);