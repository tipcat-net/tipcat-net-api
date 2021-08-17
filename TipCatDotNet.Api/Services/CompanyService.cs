using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models;
using Company = TipCatDotNet.Api.Models.Company;

namespace TipCatDotNet.Api.Services
{
    public class CompanyService : ICompanyService
    {
        public CompanyService(CustomerDbContext context)
        {
            _context = context;
        }


        public async Task Add(Company company)
        {
            _context.Companies.Add(new TipCatDotNet.Api.Data.Company
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
