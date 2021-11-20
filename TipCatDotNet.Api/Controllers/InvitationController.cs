using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services;
using TipCatDotNet.Api.Services.Auth;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api/invitations")]
    [Produces("application/json")]
    public class InvitationController : BaseController
    {
        public InvitationController(IMemberContextService memberContextService, IInvitationService invitationService)
        {
            _invitationService = invitationService;
            _memberContextService = memberContextService;
        }


        /// <summary>
        /// Sends an invitation to added member.
        /// </summary>
        [HttpPost("accounts/{accountId}/members/{memberId}/send")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send([FromRoute] int accountId, [FromRoute] int memberId)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return NoContentOrBadRequest(await _invitationService.Send(memberContext, new MemberRequest(memberId, accountId)));
        }


        private readonly IInvitationService _invitationService;
        private readonly IMemberContextService _memberContextService;
    }
}
