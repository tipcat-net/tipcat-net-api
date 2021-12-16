namespace TipCatDotNet.Api.Options;

public class CompanyInfoOptions
{
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string LegalEntity { get; set; } = null!;
    public string PostalBox { get; set; } = null!;
    public string TradeLicenseNumber { get; set; } = null!;
}