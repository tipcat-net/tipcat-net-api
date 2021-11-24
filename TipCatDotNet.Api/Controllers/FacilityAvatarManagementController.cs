using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Images;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.Images;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api/accounts/{accountId}/facilities/{facilityId}/avatar")]
[Produces("application/json")]
public class FacilityAvatarManagementController : BaseController
{
    public FacilityAvatarManagementController(IMemberContextService memberContextService,
        IAvatarManagementService<FacilityAvatarRequest> facilityAvatarManagementService)
    {
        _facilityAvatarManagementService = facilityAvatarManagementService;
        _memberContextService = memberContextService;
    }


    /// <summary>
    /// Adds new or update existing facility's avatar. A new one overwrites an old one.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="facilityId">Target facility ID</param>
    /// <param name="file">Avatar as a form file</param>
    /// <returns></returns>
    [HttpPost]
    [HttpPut]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdate([FromRoute] int accountId, [FromRoute] int facilityId, [FromForm] IFormFile? file)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        var request = new FacilityAvatarRequest(accountId, facilityId, (FormFile?) file);
        return OkOrBadRequest(await _facilityAvatarManagementService.AddOrUpdate(memberContext, request));
    }


    /// <summary>
    /// Removes facility's avatar.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="facilityId">Target facility ID</param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove([FromRoute] int accountId, [FromRoute] int facilityId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        var request = new FacilityAvatarRequest(accountId, facilityId);
        return NoContentOrBadRequest(await _facilityAvatarManagementService.Remove(memberContext, request));
    }


    private readonly IAvatarManagementService<FacilityAvatarRequest> _facilityAvatarManagementService;
    private readonly IMemberContextService _memberContextService;
}