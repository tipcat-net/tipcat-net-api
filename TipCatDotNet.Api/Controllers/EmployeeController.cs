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
    [Route("api/employees")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class EmployeeController: BaseController
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
        [ProducesResponseType(typeof(EmployeeInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get()
        {
            return Ok(new EmployeeInfoResponse(name: "test", lastName: "testov", email: "test@test.test", permission: HospitalityFacilityPermissions.Owner));
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