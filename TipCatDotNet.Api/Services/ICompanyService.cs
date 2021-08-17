using System.Collections.Generic;
using System.Threading.Tasks;
using TipCatDotNet.Api.Models;

namespace TipCatDotNet.Api.Services
{
    public interface ICompanyService
    {
        public Task Add(Company company);
        public Task<List<Company>> Get(CustomerContext customerContext);
    }
}
