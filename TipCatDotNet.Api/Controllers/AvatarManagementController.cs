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
[Route("api")]
[Produces("application/json")]
public class AvatarManagementController : BaseController
{
    public AvatarManagementController(IMemberContextService memberContextService, IAvatarManagementService avatarManagementService)
    {
        _avatarManagementService = avatarManagementService;
        _memberContextService = memberContextService;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="memberId"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("accounts/{accountId}/members/{memberId}/avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMemberAvatar([FromRoute] int accountId, [FromRoute] int memberId, [FromForm] IFormFile? file)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        var request = new MemberAvatarRequest(accountId, memberId, (FormFile?) file);
        return OkOrBadRequest(await _avatarManagementService.AddOrUpdateMemberAvatar(memberContext, request));
    }


    private readonly IAvatarManagementService _avatarManagementService;
    private readonly IMemberContextService _memberContextService;
}
