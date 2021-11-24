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
[Route("api/accounts/{accountId}/avatar")]
[Produces("application/json")]
public class AccountAvatarManagementController : BaseController
{
    public AccountAvatarManagementController(IMemberContextService memberContextService,
        IAvatarManagementService<AccountAvatarRequest> accountAvatarManagementService)
    {
        _accountAvatarManagementService = accountAvatarManagementService;
        _memberContextService = memberContextService;
    }


    /// <summary>
    /// Adds new or update existing account's avatar. A new one overwrites an old one.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="file">Avatar as a form file</param>
    /// <returns></returns>
    [HttpPost]
    [HttpPut]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdate([FromRoute] int accountId, [FromForm] IFormFile? file)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        var request = new AccountAvatarRequest(accountId, (FormFile?) file);
        return OkOrBadRequest(await _accountAvatarManagementService.AddOrUpdate(memberContext, request));
    }


    /// <summary>
    /// Removes account's avatar.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove([FromRoute] int accountId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        var request = new AccountAvatarRequest(accountId);
        return NoContentOrBadRequest(await _accountAvatarManagementService.Remove(memberContext, request));
    }


    private readonly IAvatarManagementService<AccountAvatarRequest> _accountAvatarManagementService;
    private readonly IMemberContextService _memberContextService;
}