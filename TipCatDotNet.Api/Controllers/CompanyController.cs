using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api/accounts")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class AccountController: BaseController
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
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