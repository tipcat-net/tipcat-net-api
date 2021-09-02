using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Controllers
{
    [Route("api/auth")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class AuthController: BaseController
    {
        [HttpPost("sing-in")]
        [ProducesResponseType(typeof(SignInRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignIn([FromBody]SignInRequest request)
        {
            return Ok();
        }
        
        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            return Ok();
        }
        
        
        [HttpPost("sign-up")]
        [ProducesResponseType(typeof(AccountSignUpResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignUp([FromBody]SignUpRequest request)
        {
            return Ok(new AccountSignUpResponse());
        }
    }
}