using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.Company;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api/support")]
[Produces("application/json")]
public class SupportController : BaseController
{
    public SupportController(IMemberContextService memberContextService, ISupportService supportService)
    {
        _memberContextService = memberContextService;
        _supportService = supportService;
    }


    /// <summary>
    /// Requests support from the support team. A current member receives a message as well. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("request")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Post([FromBody] SupportRequest request)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return NoContentOrBadRequest(await _supportService.SendRequest(memberContext, request));
    }


    private readonly IMemberContextService _memberContextService;
    private readonly ISupportService _supportService;
}