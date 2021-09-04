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
        [HttpPost()]
        public async Task<IActionResult> Add()
        {
            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            return Ok(); 
        }


        [HttpPut]
        public async Task<IActionResult> Update()
        {
            return Ok();
        }
    }
}