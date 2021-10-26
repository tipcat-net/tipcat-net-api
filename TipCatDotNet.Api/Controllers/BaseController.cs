using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TipCatDotNet.Api.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected BadRequestObjectResult BadRequest(string error)
            => BadRequest(new ProblemDetails
            {
                Detail = error,
                Status = StatusCodes.Status400BadRequest
            });

        
        protected IActionResult NoContentOrBadRequest(Result result)
            => result.IsSuccess
                ? NoContent()
                : BadRequest(result.Error);

        
        protected IActionResult NoContentOrBadRequest<T>(Result<T> result)
            => result.IsSuccess
                ? NoContent()
                : BadRequest(result.Error);


        protected NotFoundObjectResult NotFound(string? error)
            => NotFound(new ProblemDetails
            {
                Detail = error,
                Status = StatusCodes.Status404NotFound
            });

        
        protected IActionResult OkOrBadRequest<T>(Result<T> result)
            => result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
    }
}
