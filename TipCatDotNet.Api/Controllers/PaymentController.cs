using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Controllers
{
    [Route("api/payments")]
    [Produces("application/json")]
    public class PaymentController : BaseController
    {
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        /// <summary>
        /// Prepare payment and get details by member code.
        /// </summary>
        /// <param name="memberCode">Member Code</param>
        /// <returns></returns>
        [HttpGet("{memberCode}/prepare")]
        [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Prepare(string memberCode)
            => OkOrBadRequest(await _paymentService.GetMemberDetails(memberCode));


        /// <summary>
        /// Get payment details by id.
        /// </summary>
        /// <param name="paymentId">Payment id</param>
        /// <returns></returns>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(string paymentId)
            => OkOrBadRequest(await _paymentService.Get(paymentId));


        /// <summary>
        /// Proceed to payment.
        /// </summary>
        /// <param name="paymentRequest">Payment request</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Pay([FromBody] PaymentRequest paymentRequest)
            => NoContentOrBadRequest(await _paymentService.Pay(paymentRequest));


        private readonly IPaymentService _paymentService;
    }
}