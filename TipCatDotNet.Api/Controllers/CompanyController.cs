using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api/company")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class CompanyController: BaseController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok();
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetMyCompanies()
        {
            return Ok();
        }
        
        [HttpPost("add")]
        public async Task<IActionResult> Add()
        {
            return Ok();
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update()
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