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
    [Route("api/employee")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class EmployeeController: BaseController
    {
        [HttpPost("add")]
        public async Task<IActionResult> Add()
        {
            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(EmployeeInfoResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get()
        {
            return Ok(new EmployeeInfoResponseModel(name: "test", lastName: "testov", email: "test@test.test", permission: HospitalityFacilityPermissions.Owner));
        }
        
        [HttpPost("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok();
        }
        
        [HttpDelete("{id}/remove")]
        public async Task<IActionResult> Remove(int id)
        {
            return Ok();
        }
    }
}