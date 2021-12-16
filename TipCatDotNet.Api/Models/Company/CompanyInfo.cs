namespace TipCatDotNet.Api.Models.Company;

public readonly record struct CompanyInfo(string LegalEntity, string Country, string City, string Address, string PostalBox, string TradeLicenseNumber);