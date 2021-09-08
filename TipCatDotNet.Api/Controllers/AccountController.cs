using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
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


        [HttpPost]
        public async Task<IActionResult> Add()
        {
            return Ok();
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
        }


        [HttpPut]
        public async Task<IActionResult> Update()
        {
            return Ok();
        }*/


        private readonly IAccountService _accountService;
        private readonly IMemberContextService _memberContextService;
    }
}