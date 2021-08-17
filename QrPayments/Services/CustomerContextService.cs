using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Data;
using TipCatDotNet.Models;

namespace TipCatDotNet.Services
{
    public class CustomerContextService : ICustomerContextService
    {
        public CustomerContextService(CustomerDbContext context)
        {
            _context = context;
        }


        public async Task<Result<CustomerContext>> Get(int customerId)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == customerId)
                .Select(c => new CustomerContext(c.Id))
                .SingleOrDefaultAsync();

            return customer.Equals(default) 
                ? Result.Failure<CustomerContext>($"Can't find a customer with ID {customerId}") 
                : Result.Success(customer);
        }
    
        
        private readonly CustomerDbContext _context;
    }
}
