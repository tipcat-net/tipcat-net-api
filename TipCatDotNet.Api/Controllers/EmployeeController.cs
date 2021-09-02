using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace TipCatDotNet.Api.Controllers
{
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