using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Controllers;

[Route("api/payments")]
[Produces("application/json")]
public class PaymentController : BaseController
{
    public PaymentController(IPaymentService paymentService, IPayoutService payoutService)
    {
        _paymentService = paymentService;
        _payoutService = payoutService;
    }


    /// <summary>
    /// Gets payment details by a member code.
    /// </summary>
    /// <param name="memberCode">Member Code</param>
    /// <returns></returns>
    [HttpGet("{memberCode}")]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Prepare([FromRoute] string memberCode)
        => OkOrBadRequest(await _paymentService.Get(memberCode));


    /// <summary>
    /// Proceed to a payment.
    /// </summary>
    /// <param name="paymentRequest">Payment request</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pay([FromBody] PaymentRequest paymentRequest)
        => OkOrBadRequest(await _paymentService.Pay(paymentRequest));


    /// <summary>
    /// Capture a payment by an ID.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns></returns>
    [HttpPost("{paymentId}/capture")]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Capture([FromRoute] string paymentId)
        => OkOrBadRequest(await _paymentService.Capture(paymentId));


    /// <summary>
    /// Update a payment by ID.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="paymentRequest">Payment request</param>
    /// <returns></returns>
    [HttpPut("{paymentId}")]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] string paymentId, [FromBody] PaymentRequest paymentRequest)
        => OkOrBadRequest(await _paymentService.Update(paymentId, paymentRequest));


    /// <summary>
    /// Processes payments.
    /// </summary>
    /// <returns></returns>
    [HttpPost("status/handle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleStatus()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        return NoContentOrBadRequest(await _paymentService.ProcessChanges(json, Request.Headers[SignatureHeader]));
    }


    /// <summary>
    /// Pays out captured tips.
    /// </summary>
    /// <returns></returns>
    [HttpPost("payout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Payout()
        => NoContentOrBadRequest(await _payoutService.PayOut());


    private const string SignatureHeader = "Stripe-Signature";
    private readonly IPaymentService _paymentService;
    private readonly IPayoutService _payoutService;
}