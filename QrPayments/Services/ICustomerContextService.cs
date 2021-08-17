using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Models;

namespace TipCatDotNet.Services
{
    public interface ICustomerContextService
    {
        Task<Result<CustomerContext>> Get(int customerId);
    }
}
