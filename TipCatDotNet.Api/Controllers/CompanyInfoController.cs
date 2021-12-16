using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Services.Company;

namespace TipCatDotNet.Api.Controllers;

[Route("api/company-info")]
[Produces("application/json")]
public class CompanyInfoController : BaseController
{
    /// <summary>
    /// Gets company's legal information.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(CompanyInfo), StatusCodes.Status200OK)]
    public IActionResult Get() 
        => Ok(CompanyInfoService.Get);
}