using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Models;

namespace TipCatDotNet.Api.Controllers
{
    [Route("api/account")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class AccountController : BaseController
    {
        
        [Authorize]
        [HttpPost("sign-up-step-2")]
        public async Task<IActionResult> SignUpStep2()
        {
            return Ok();
        }
        
        [Authorize]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }
        
    }
}