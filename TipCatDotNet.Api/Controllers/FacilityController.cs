using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api/accounts/{accountId:int}")]
[Produces("application/json")]
public class FacilityController : BaseController
{
    public FacilityController(IMemberContextService memberContextService, ITransactionService transactionService, IFacilityService facilityService)
    {
        _facilityService = facilityService;
        _memberContextService = memberContextService;
        _transactionService = transactionService;
    }


    /// <summary>
    /// Adds a facility to a target account.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="request">Facility details</param>
    /// <returns></returns>
    [HttpPost("facilities")]
    [ProducesResponseType(typeof(FacilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromRoute] int accountId, [FromBody] FacilityRequest request)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _facilityService.Add(memberContext, new FacilityRequest(null, accountId, request)));
    }


    /// <summary>
    /// Transfers a member to a facility within an account.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="facilityId">Target facility ID</param>
    /// <param name="memberId">Target member ID</param>
    /// <returns></returns>
    [HttpPost("members/{memberId:int}/transfer/facilities/{facilityId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransferMember([FromRoute] int accountId, [FromRoute] int facilityId, [FromRoute] int memberId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return NoContentOrBadRequest(await _facilityService.TransferMember(memberContext, memberId, facilityId, accountId));
    }


    /// <summary>
    /// Updates an existing facility.
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <param name="facilityId">Facility ID</param>
    /// <param name="request">Facility details</param>
    /// <returns></returns>
    [HttpPut("facilities/{facilityId:int}")]
    [ProducesResponseType(typeof(FacilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] int accountId, [FromRoute] int facilityId,
        [FromBody] FacilityRequest request)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _facilityService.Update(memberContext, new FacilityRequest(facilityId, accountId, request)));
    }


    /// <summary>
    /// Gets transactions pagination by a facility ID.
    /// </summary>
    /// <param name="facilityId">Target facility id</param>
    /// <returns></returns>
    [HttpGet("facilities/{facilityId:int}/transactions")]
    [EnableQuery]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTransactions([FromRoute] int facilityId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _transactionService.Get(memberContext, facilityId));
    }


    private readonly IFacilityService _facilityService;
    private readonly ITransactionService _transactionService;
    private readonly IMemberContextService _memberContextService;
}