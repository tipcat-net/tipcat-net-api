using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Data;
using TipCatDotNet.Models;
using Customer = TipCatDotNet.Models.Customer;

namespace TipCatDotNet.Services
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
