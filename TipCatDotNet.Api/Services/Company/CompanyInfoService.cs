using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.Company;

public class CompanyInfoService : ICompanyInfoService
{
    public CompanyInfoService(IOptions<CompanyInfoOptions> options)
    {
        _options = options.Value;
    }


    public CompanyInfo Get()
    {
        if (_instance is not null)
            return _instance.Value;
	
        _instance = new CompanyInfo(_options.LegalEntity, _options.Country, _options.City, _options.Address, _options.PostalBox, _options.TradeLicenseNumber);
        return _instance.Value;
    }


    private static CompanyInfo? _instance;
    
    private readonly CompanyInfoOptions _options;
}