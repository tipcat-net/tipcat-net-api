using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments.Enums;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api")]
[Produces("application/json")]
public class MemberController : BaseController
{
    public MemberController(IMemberContextService memberContextService, IMemberService memberService)
    {
        _memberContextService = memberContextService;
        _memberService = memberService;
    }


    /// <summary>
    /// Activates a member.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpPost("accounts/{accountId:int}/members/{memberId:int}/activate")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate([FromRoute] int memberId, [FromRoute] int accountId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _memberService.Activate(memberContext, new MemberRequest(memberId, accountId)));
    }


    /// <summary>
    /// Adds a member to an account.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="memberRequest">Change request</param>
    /// <returns></returns>
    [HttpPost("accounts/{accountId:int}/members")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Add([FromRoute] int accountId, [FromBody] MemberRequest memberRequest)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _memberService.Add(memberContext, new MemberRequest(null, accountId, memberRequest)));
    }


    /// <summary>
    /// Creates a current member from registration details. Suitable for account managers only.
    /// </summary>
    /// <returns></returns>
    [HttpPost("members/current")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddCurrent()
        => OkOrBadRequest(await _memberService.AddCurrent(User.GetId()));


    /// <summary>
    /// Deactivates a member.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [HttpPost("accounts/{accountId:int}/members/{memberId:int}/deactivate")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate([FromRoute] int memberId, [FromRoute] int accountId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _memberService.Deactivate(memberContext, new MemberRequest(memberId, accountId)));
    }


    /// <summary>
    /// Gets a current member.
    /// </summary>
    /// <returns></returns>
    [HttpGet("members/current")]
    [ProducesResponseType(typeof(MemberResponse?), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrent()
    {
        var (_, isFailure, context, error) = await _memberContextService.Get();
        if (isFailure)
            return NotFound(error);

        return OkOrBadRequest(await _memberService.GetCurrent(context!));
    }


    /// <summary>
    /// Regenerate member's qr code.
    /// </summary>
    /// <param name="memberId">Target member ID</param>
    /// <param name="accountId">Target account ID</param>
    /// <returns></returns>
    [HttpGet("accounts/{accountId:int}/members/{memberId:int}/qr-code/generate")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegenerateQr([FromRoute] int memberId, [FromRoute] int accountId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _memberService.RegenerateQr(memberContext, memberId, accountId));
    }


    /// <summary>
    /// Removes a member from an account.
    /// </summary>
    /// <param name="memberId">Target member ID</param>
    /// <param name="accountId">Target account ID</param>
    /// <returns></returns>
    [HttpDelete("accounts/{accountId:int}/members/{memberId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remove([FromRoute] int memberId, [FromRoute] int accountId)
    {
        var (_, isMemberExists, context, error) = await _memberContextService.Get();
        if (isMemberExists)
            return NotFound(error);

        return NoContentOrBadRequest(await _memberService.Remove(context, memberId, accountId));
    }


    /// <summary>
    /// Updates a member of an account.
    /// </summary>
    /// <param name="memberId">Target member ID</param>
    /// <param name="accountId">Target account ID</param>
    /// <param name="request">Change request</param>
    /// <returns></returns>
    [HttpPut("accounts/{accountId:int}/members/{memberId:int}")]
    [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrent([FromRoute] int memberId, [FromRoute] int accountId, [FromBody] MemberRequest request)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _memberService.Update(memberContext, new MemberRequest(memberId, accountId, request)));
    }


    /// <summary>
    /// Updates member's active stripe account.
    /// </summary>
    /// <param name="memberId">Target member ID</param>
    /// <param name="accountId">Target account ID</param>
    /// <param name="accountType">Type of active stripe account</param>
    /// <returns></returns>
    [HttpPut("accounts/{accountId:int}/members/{memberId:int}/stripe-account/set-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update([FromRoute] int memberId, [FromRoute] int accountId,
        [FromQuery] ActiveStripeAccountType accountType)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return NotFound(error);

        return NoContentOrBadRequest(await _memberService.Update(memberContext, new MemberRequest(memberId, accountId), accountType));
    }


    private readonly IMemberContextService _memberContextService;
    private readonly IMemberService _memberService;
}