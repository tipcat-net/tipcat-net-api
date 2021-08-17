using System.Collections.Generic;
using System.Threading.Tasks;
using TipCatDotNet.Models;

namespace TipCatDotNet.Services
{
    public interface ICompanyService
    {
        public Task Add(Company company);
        public Task<List<Company>> Get(CustomerContext customerContext);
    }
}
