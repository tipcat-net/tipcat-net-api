using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TipCatDotNet.Api.Services;

namespace TipCatDotNet.Api.Controllers
{
    [ApiController]
    [Route("companies")]
    public class CompanyController : BaseController
    {
        public CompanyController(ICompanyService service, ICustomerContextService customerContext)
        {
            _customerContext = customerContext;
            _service = service;
        }


        [Authorize]
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
        private readonly ICompanyService _service;
    }
}
