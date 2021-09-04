using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api/members")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class MemberController: BaseController
    {
        [HttpPost("add")]
        [ProducesResponseType( StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Add()
        {
            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(MemberInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get()
        {
            return Ok(new MemberInfoResponse(name: "test", lastName: "testov", email: "test@test.test", permissions: MemberPermissions.Manager));
        }
        
        [HttpPost("{id}")]
        [ProducesResponseType( StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get(int id)
        {
            return Ok();
        }
        
        [HttpDelete("{id}/remove")]
        [ProducesResponseType( StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Remove(int id)
        {
            return Ok();
        }
    }
}