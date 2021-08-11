using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QrPayments.Data;
using QrPayments.Models;
using Company = QrPayments.Models.Company;

namespace QrPayments.Services
{
    public class CompanyService : ICompanyService
    {
        public CompanyService(CustomerDbContext context)
        {
            _context = context;
        }


        public async Task Add(Company company)
        {
            _context.Companies.Add(new Data.Company
            {
                Name = company.Name
            });

            await _context.SaveChangesAsync();
        }


        public async Task<List<Company>> Get(CustomerContext customerContext)
        {
            return await _context.Companies
                .Select(x => new Company(x.Id, x.Name))
                .ToListAsync();
        }
    
        
        private readonly CustomerDbContext _context;
    }
}
