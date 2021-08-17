using System.Threading.Tasks;
using TipCatDotNet.Models;

namespace TipCatDotNet.Services
{
    public interface ICustomerService
    {
        Task<Customer> Get(CustomerContext customerContext);
    }
}
