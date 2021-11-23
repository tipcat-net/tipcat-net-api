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
[Route("api/accounts/{accountId}/members/{memberId}/avatar")]
[Produces("application/json")]
public class MemberAvatarManagementController : BaseController
{
    public MemberAvatarManagementController(IMemberContextService memberContextService,
        IAvatarManagementService<MemberAvatarRequest> memberAvatarManagementService)
    {
        _memberAvatarManagementService = memberAvatarManagementService;
        _memberContextService = memberContextService;
    }


    /// <summary>
    /// Adds new or update existing member's avatar. A new one overwrites an old one.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="memberId">Target member ID</param>
    /// <param name="file">Avatar as a form file</param>
    /// <returns></returns>
    [HttpPost]
    [HttpPut]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateMemberAvatar([FromRoute] int accountId, [FromRoute] int memberId, [FromForm] IFormFile? file)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        var request = new MemberAvatarRequest(accountId, memberId, (FormFile?) file);
        return OkOrBadRequest(await _memberAvatarManagementService.AddOrUpdate(memberContext, request));
    }


    /// <summary>
    /// Removes member's avatar.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="memberId">Target member ID</param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove([FromRoute] int accountId, [FromRoute] int memberId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        var request = new MemberAvatarRequest(accountId, memberId);
        return NoContentOrBadRequest(await _memberAvatarManagementService.Remove(memberContext, request));
    }


    private readonly IAvatarManagementService<MemberAvatarRequest> _memberAvatarManagementService;
    private readonly IMemberContextService _memberContextService;
}