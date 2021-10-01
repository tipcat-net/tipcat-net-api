using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/payments")]
    [Produces("application/json")]
    [RequiredScope(ScopeRequiredByApi)]
    public class PaymentController : BaseController
    {
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        /// <summary>
        /// Get payment details by member code.
        /// </summary>
        /// <param name="memberCode">Member Code</param>
        /// <returns></returns>
        [HttpGet("{memberCode}/prepare")]
        [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(string memberCode)
            => NoContentOrBadRequest(await _paymentService.GetDetails(memberCode));


        /// <summary>
        /// Proceed to payment.
        /// </summary>
        /// <param name="paymentRequest">Payment request</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Pay([FromBody] PaymentRequest paymentRequest) =>
            OkOrBadRequest(await _paymentService.Pay(paymentRequest));


        private readonly IPaymentService _paymentService;
    }
}