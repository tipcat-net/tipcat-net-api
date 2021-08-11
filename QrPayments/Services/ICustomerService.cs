using System.Threading.Tasks;
using QrPayments.Models;

namespace QrPayments.Services
{
    public interface ICustomerService
    {
        Task<Customer> Get(CustomerContext customerContext);
    }
}
