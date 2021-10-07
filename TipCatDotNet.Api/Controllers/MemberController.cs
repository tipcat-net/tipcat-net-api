using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class MemberController : BaseController
    {
        public MemberController(IMemberContextService memberContextService, IMemberService memberService)
        {
            _memberContextService = memberContextService;
            _memberService = memberService;
        }


        /// <summary>
        /// Adds a member to an account.
        /// </summary>
        /// <param name="accountId">Target account ID</param>
        /// <param name="memberRequest">Change request</param>
        /// <returns></returns>
        [HttpPost("accounts/{accountId}/members")]
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
        /// Adds a member to a facility into an account.
        /// </summary>
        /// <param name="accountId">Target account ID</param>
        /// <param name="facilityId">Target facility ID</param>
        /// <param name="memberRequest">Change request</param>
        /// <returns></returns>
        [HttpPost("accounts/{accountId}/facilities/{facilityId}/members")]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddFacility([FromRoute] int accountId, [FromRoute] int facilityId, [FromBody] MemberRequest memberRequest)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return NoContentOrBadRequest(await _memberService.TransferToFacility(memberContext, facilityId, new MemberRequest(null, accountId, memberRequest)));
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
        /// Regenerate member's qr code.
        /// </summary>
        /// <param name="memberId">Target member ID</param>
        /// <param name="accountId">Target account ID</param>
        /// <returns></returns>
        [HttpGet("accounts/{accountId}/members/{memberId}/qr-code/generate")]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegenerateQR([FromRoute] int memberId, [FromRoute] int accountId)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _memberService.RegenerateQR(memberContext, memberId, accountId));
        }


        /// <summary>
        /// Gets all member of an account.
        /// </summary>
        /// <param name="accountId">Target account ID</param>
        /// <returns></returns>
        [HttpGet("accounts/{accountId}/members")]
        [ProducesResponseType(typeof(List<MemberResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get([FromRoute] int accountId)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _memberService.Get(memberContext, accountId));
        }


        /// <summary>
        /// Gets a member of an account by ID.
        /// </summary>
        /// <param name="memberId">Target member ID</param>
        /// <param name="accountId">Target account ID</param>
        /// <returns></returns>
        [HttpGet("accounts/{accountId}/members/{memberId}")]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get([FromRoute] int memberId, [FromRoute] int accountId)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _memberService.Get(memberContext, memberId, accountId));
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
            var (_, isMemberExists, context, error) = await _memberContextService.Get();
            if (isMemberExists)
                return NotFound(error);

            return OkOrBadRequest(await _memberService.GetCurrent(context!));
        }


        /// <summary>
        /// Removes a member from an account.
        /// </summary>
        /// <param name="memberId">Target member ID</param>
        /// <param name="accountId">Target account ID</param>
        /// <returns></returns>
        [HttpDelete("accounts/{accountId}/members/{memberId}")]
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
        [HttpPut("accounts/{accountId}/members/{memberId}")]
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
        /// Updates a avatar current member from registration details. Suitable for account managers only.
        /// </summary>
        /// <returns></returns>
        /*[HttpPut("members/current/avatar")]
        [ProducesResponseType(typeof(MemberInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAvatar(MemberAvatarRequest request) 
            => OkOrBadRequest(await _memberService.UpdateAvatar(User.GetId(), request));*/


        private readonly IMemberContextService _memberContextService;
        private readonly IMemberService _memberService;
    }
}