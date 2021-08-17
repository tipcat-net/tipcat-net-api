using System.Threading.Tasks;
using TipCatDotNet.Api.Models;

namespace TipCatDotNet.Api.Services
{
    public interface ICustomerService
    {
        Task<Customer> Get(CustomerContext customerContext);
    }
}
