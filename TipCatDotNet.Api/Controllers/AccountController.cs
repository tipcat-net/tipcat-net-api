using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api/accounts")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class AccountController: BaseController
    {
        public AccountController(IMemberContextService memberContextService, IAccountService accountService)
        {
            _accountService = accountService;
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


        /*[HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }*/


        /// <summary>
        /// Gets an account by ID.
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns></returns>
        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(int accountId)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _accountService.Get(memberContext, accountId));
        }


        /*[HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            return Ok(); 
        }*/


        /// <summary>
        /// Updates an existing account.
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="request">Account details</param>
        /// <returns></returns>
        [HttpPut("{accountId}")]
        [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] int accountId, [FromBody] AccountRequest request)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _accountService.Update(memberContext,
                new AccountRequest(accountId, request.Address, request.CommercialName, request.Email, request.Name, request.Phone)));
        }


        private readonly IAccountService _accountService;
        private readonly IMemberContextService _memberContextService;
    }
}