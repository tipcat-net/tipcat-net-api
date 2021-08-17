using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models;
using Customer = TipCatDotNet.Api.Models.Customer;

namespace TipCatDotNet.Api.Services
{
    public class CustomerService : ICustomerService
    {
        public CustomerService(CustomerDbContext context)
        {
            _context = context;
        }


        public async Task<Customer> Get(CustomerContext customerContext)
            => await _context.Customers
                .Where(c => c.Id == customerContext.Id)
                .Select(c => new Customer(c.Id, c.Name, c.Email))
                .SingleAsync();


        private readonly CustomerDbContext _context;
    }
}
