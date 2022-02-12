using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TipCatDotNet.Api.Models.Analitics;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.Analitics;

namespace TipCatDotNet.Api.Controllers;

[Authorize]
[Route("api/accounts/{accountId:int}/stats")]
[Produces("application/json")]
public class AccountStatsController : BaseController
{
    public AccountStatsController(IMemberContextService memberContextService, IAccountStatsService accountStatsService)
    {
        _accountStatsService = accountStatsService;
        _memberContextService = memberContextService;
    }


    /// <summary>
    /// Gets facilities stats for a target account
    /// </summary>
    /// <param name="accountId">Target account ID</param>
    /// <returns></returns>
    [HttpGet]
    [EnableQuery]
    [ProducesResponseType(typeof(List<AccountStatsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStats([FromRoute] int accountId)
    {
        var (_, isFailure, memberContext, error) = await _memberContextService.Get();
        if (isFailure)
            return BadRequest(error);

        return OkOrBadRequest(await _accountStatsService.Get(memberContext, accountId));
    }

    
    private readonly IAccountStatsService _accountStatsService;
    private readonly IMemberContextService _memberContextService;
}