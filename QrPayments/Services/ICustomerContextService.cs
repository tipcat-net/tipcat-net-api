using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using QrPayments.Models;

namespace QrPayments.Services
{
    public interface ICustomerContextService
    {
        Task<Result<CustomerContext>> Get(int customerId);
    }
}
