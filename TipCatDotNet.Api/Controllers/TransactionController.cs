using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Common.Enums;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionController : BaseController
{
    public TransactionController(IMemberContextService memberContextService, ITransactionService transactionService)
    {
        _transactionService = transactionService;
        _memberContextService = memberContextService;
    }


    /// <summary>
    /// Gets transactions pagination by member.
    /// </summary>
    /// <param name="skip">The number of skipped transactions</param>
    /// <param name="top">The number of received transactions </param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery][Range(0, int.MaxValue)] int skip = 0,
        [FromQuery][Range(0, 100)] int top = 20)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _transactionService.Get(memberContext, skip, top));
    }


    /// <summary>
    /// Gets sorted transactions by amount.
    /// </summary>
    /// <param name="skip">The number of skipped transactions</param>
    /// <param name="top">The number of received transactions </param>
    /// <param name="variant">The variant of sorting transactions by amount</param>
    /// <returns></returns>
    [HttpGet("sort/amount")]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByAmount([FromQuery][Range(0, int.MaxValue)] int skip = 0,
        [FromQuery][Range(0, 100)] int top = 20, [FromQuery] SortVariant variant = SortVariant.DESC)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _transactionService.SortByAmount(memberContext, skip, top, variant));
    }


    private readonly ITransactionService _transactionService;
    private readonly IMemberContextService _memberContextService;
}