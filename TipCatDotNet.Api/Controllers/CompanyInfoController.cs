using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Services.Company;

namespace TipCatDotNet.Api.Controllers;

[Route("api/company-info")]
[Produces("application/json")]
public class CompanyInfoController : BaseController
{
    public CompanyInfoController(ICompanyInfoService companyInfoService)
    {
        _companyInfoService = companyInfoService;
    }


    /// <summary>
    /// Gets company's legal information.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(CompanyInfo), StatusCodes.Status200OK)]
    public IActionResult Get() 
        => Ok(_companyInfoService.Get());

    
    private readonly ICompanyInfoService _companyInfoService;
}