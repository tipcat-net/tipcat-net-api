using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Preferences;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.Preferences;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api/members/current/preferences")]
[Produces("application/json")]
public class MemberPreferencesController : BaseController
{
    public MemberPreferencesController(IMemberContextService memberContextService, IPreferencesService memberPreferencesService)
    {
        _memberContextService = memberContextService;
        _preferencesService = memberPreferencesService;
    }


    /// <summary>
    /// Sets or updates member preferences.
    /// </summary>
    /// <param name="request">Preferences</param>
    /// <returns></returns>
    [HttpPost]
    [HttpPut]
    [RequestSizeLimit(256 * 1024)]
    [ProducesResponseType(typeof(PreferencesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdate([FromBody] PreferencesRequest request)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _preferencesService.AddOrUpdate(memberContext, request));
    }


    /// <summary>
    /// Gets member preferences.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(PreferencesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return Ok(await _preferencesService.Get(memberContext));
    }

    
    private readonly IMemberContextService _memberContextService;
    private readonly IPreferencesService _preferencesService;
}