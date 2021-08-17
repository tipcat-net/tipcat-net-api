using Microsoft.AspNetCore.Mvc;
using System.Linq;
using CSharpFunctionalExtensions;

namespace QrPayments.Controllers
{
    public class BaseController : ControllerBase
    {
        protected Result<int> GetUserId()
            => Request.Headers.TryGetValue(UserIdHeaderName, out var values)
                ? Result.Success(int.Parse(values.First()))
                : Result.Failure<int>("A user ID header is missing.");


        protected IActionResult OkOrBadRequest<T>(Result<T> result)
            => result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);


        private const string UserIdHeaderName = "X-User-ID";
        protected const string ScopeRequiredByApi = "access_as_service_provider";
    }
}
