using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Controllers
{
    [Authorize]
    [Route("api")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class FacilityController : BaseController
    {
        public FacilityController(IMemberContextService memberContextService, IFacilityService facilityService)
        {
            _facilityService = facilityService;
            _memberContextService = memberContextService;
        }


        /// <summary>
        /// Adds facility to a target account
        /// </summary>
        /// <param name="accountId">Target account ID</param>
        /// <param name="request">Facility details</param>
        /// <returns></returns>
        [HttpPost("accounts/{accountId}/facilities")]
        [ProducesResponseType(typeof(FacilityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromRoute] int accountId, [FromBody] FacilityRequest request)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _facilityService.Add(memberContext, new FacilityRequest(null, accountId, request)));
        }


        /// <summary>
        /// Gets a facility by ID.
        /// </summary>
        /// <param name="accountId">Target account ID</param>
        /// <param name="facilityId">Facility ID</param>
        /// <returns></returns>
        [HttpGet("accounts/{accountId}/facilities/{facilityId}")]
        [ProducesResponseType(typeof(FacilityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(int accountId, int facilityId)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _facilityService.Get(memberContext, facilityId, accountId));
        }


        /// <summary>
        /// Updates an existing facility.
        /// </summary>
        /// <param name="facilityId">Facility ID</param>
        /// <param name="request">Facility details</param>
        /// <returns></returns>
        [HttpPut("accounts/{accountId}/facilities/{facilityId}")]
        [ProducesResponseType(typeof(FacilityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] int facilityId, [FromBody] FacilityRequest request)
        {
            var (_, isFailure, memberContext, error) = await _memberContextService.Get();
            if (isFailure)
                return BadRequest(error);

            return OkOrBadRequest(await _facilityService.Update(memberContext,
                new FacilityRequest(facilityId, request)));
        }


        private readonly IFacilityService _facilityService;
        private readonly IMemberContextService _memberContextService;
    }
}