using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class MemberController: BaseController
    {
        public MemberController(IMemberContextService memberContextService, IMemberService memberService)
        {
            _memberContextService = memberContextService;
            _memberService = memberService;
        }


        /// <summary>
        /// Adds or updates a member to an account.
        /// </summary>
        /// <param name="memberRequest">Member request</param>
        /// <param name="accountId">Account ID</param>
        /// <returns></returns>
        [HttpPost("accounts/{accountId}/members")]
        [ProducesResponseType(typeof(MemberInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Add([FromBody] MemberInfoResponse memberRequest, [FromRoute] int accountId)
        {
            return Ok(memberRequest);
        }
        
        
        /// <summary>
        /// Creates a current member from registration details. Suitable for account managers only.
        /// </summary>
        /// <returns></returns>
        [HttpPost("members/current")]
        [ProducesResponseType(typeof(MemberInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddCurrent() 
            => OkOrBadRequest(await _memberService.AddCurrent(User.GetId(), MemberPermissions.Manager));


        /// <summary>
        /// Gets a member of an account by ID.
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="accountId">Account ID</param>
        /// <returns></returns>
        [HttpPost("accounts/{accountId}/members/{memberId}")]
        [ProducesResponseType(typeof(MemberInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get([FromRoute] int memberId, [FromRoute] int accountId)
        {
            return Ok();
        }


        /// <summary>
        /// Gets a current member.
        /// </summary>
        /// <returns></returns>
        [HttpGet("members/current")]
        [ProducesResponseType(typeof(MemberInfoResponse?), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrent()
        {
            var (_, isMemberExists, context) = await _memberContextService.Get();
            if (isMemberExists)
                return NotFound();

            return OkOrBadRequest(await _memberService.GetCurrent(context!));
        }


        /// <summary>
        /// Removes a member from an account.
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="accountId">Account ID</param>
        /// <returns></returns>
        [HttpDelete("accounts/{accountId}/members/{memberId}")]
        [ProducesResponseType( StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Remove([FromRoute] int memberId, [FromRoute] int accountId)
        {
            return NoContent();
        }

        /// <summary>
        /// Updates a avatar current member from registration details. Suitable for account managers only.
        /// </summary>
        /// <returns></returns>
        [HttpPut("members/current/avatar")]
        [ProducesResponseType(typeof(MemberInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAvatar(MemberAvatarRequest request)
        {
            var (_, isMemberExists, context) = await _memberContextService.Get();
            if (isMemberExists)
                return NotFound();
            
            return OkOrBadRequest(await _memberService.UpdateAvatar(context, request));
        }
            

        /// <summary>
        /// Updates a current member from registration details. Suitable for account managers only.
        /// </summary>
        /// <returns></returns>
        [HttpPut("members/current")]
        [ProducesResponseType(typeof(MemberInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCurrent(MemberUpdateRequest request)
        {
            var (_, isMemberExists, context) = await _memberContextService.Get();
            if (isMemberExists)
                return NotFound();
            
            return OkOrBadRequest(await _memberService.UpdateCurrent(context, request));
        }
            
        

        private readonly IMemberContextService _memberContextService;
        private readonly IMemberService _memberService;
    }
}