using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace TipCatDotNet.Controllers
{
    [Authorize]
    [Route("api/tests")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class TestController : BaseController
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Get() => Ok("Test Passed");
    }
}
