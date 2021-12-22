using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Enums;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api/accounts")]
[Produces("application/json")]
public class AccountController : BaseController
{
    public AccountController(IMemberContextService memberContextService, IAccountService accountService, ITransactionService transactionService)
    {
        _accountService = accountService;
        _transactionService = transactionService;
        _memberContextService = memberContextService;
    }


    /// <summary>
    /// Adds an account to a current member if they don't have any.
    /// </summary>
    /// <param name="request">Account details</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AccountRequest request)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _accountService.Add(memberContext, request));
    }


    /// <summary>
    /// Gets an account by ID.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns></returns>
    [HttpGet("{accountId:int}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromRoute] int accountId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _accountService.Get(memberContext, accountId));
    }


    /// <summary>
    /// Updates an existing account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="request">Account details</param>
    /// <returns></returns>
    [HttpPut("{accountId:int}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] int accountId, [FromBody] AccountRequest request)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _accountService.Update(memberContext,
            new AccountRequest(accountId, request)));
    }


    /// <summary>
    /// Gets default amount of transactions per facilities by current account.
    /// </summary>
    /// <param name="filterProperty">The transaction's property by which it filters</param>
    /// <returns></returns>
    [HttpGet("/current/transactions")]
    [ProducesResponseType(typeof(List<FacilityTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDefaultPerFacilities([FromQuery] TransactionFilterProperty filterProperty = TransactionFilterProperty.CreatedDESC)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _transactionService.Get(memberContext, filterProperty));
    }


    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly IMemberContextService _memberContextService;
}