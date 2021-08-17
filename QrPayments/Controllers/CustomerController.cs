using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Services;

namespace TipCatDotNet.Controllers
{
    [ApiController]
    [Route("customers")]
    public class CustomerController : BaseController
    {
        public CustomerController(ICustomerService service, ICustomerContextService customerContext)
        {
            _customerContext = customerContext;
            _service = service;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var customerContext = await GetUserId()
                .Bind(async id => await _customerContext.Get(id));

            if (customerContext.IsFailure)
                return BadRequest(customerContext.Error);

            return Ok(await _service.Get(customerContext.Value));
        }
    
        
        private readonly ICustomerContextService _customerContext;
        private readonly ICustomerService _service;
    }
}
