using System.Collections.Generic;
using System.Threading.Tasks;
using QrPayments.Models;

namespace QrPayments.Services
{
    public interface ICompanyService
    {
        public Task Add(Company company);
        public Task<List<Company>> Get(CustomerContext customerContext);
    }
}
