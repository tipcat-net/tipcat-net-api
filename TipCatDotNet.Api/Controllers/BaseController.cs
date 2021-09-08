﻿using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TipCatDotNet.Api.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        public BadRequestObjectResult BadRequest(string error)
            => BadRequest(new ProblemDetails
            {
                Detail = error,
                Status = StatusCodes.Status400BadRequest
            });


        public NotFoundObjectResult NotFound(string? error)
            => NotFound(new ProblemDetails
            {
                Detail = error,
                Status = StatusCodes.Status404NotFound
            });


        protected IActionResult OkOrBadRequest<T>(Result<T> result)
            => result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);


        protected const string ScopeRequiredByApi = "access_as_service_provider";
    }
}
