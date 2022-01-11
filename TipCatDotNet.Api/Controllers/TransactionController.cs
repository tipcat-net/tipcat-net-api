using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TipCatDotNet.Api.Infrastructure.Constants;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Enums;
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
    /// <returns></returns>
    [HttpGet]
    [EnableQuery]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get()
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _transactionService.Get(memberContext));
    }


    private readonly ITransactionService _transactionService;
    private readonly IMemberContextService _memberContextService;
}