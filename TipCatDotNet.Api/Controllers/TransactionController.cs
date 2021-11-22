using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Filters.Pagination;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Controllers
{
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
        /// Gets transactions by member.
        /// </summary>
        /// <returns></returns>
        [HttpGet("last")]
        [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLast()
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _transactionService.Get(memberContext, new PaginationFilter(lastPageDesc, countLastTransactions)));
        }


        /// <summary>
        /// Gets transactions pagination by member.
        /// </summary>
        /// <param name="filter">Pagination filter</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromQuery] PaginationFilter filter)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _transactionService.Get(memberContext, filter));
        }


        private const int lastPageDesc = 1;
        private const int countLastTransactions = 10;
        private readonly ITransactionService _transactionService;
        private readonly IMemberContextService _memberContextService;
    }
}