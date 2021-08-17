using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models;

namespace TipCatDotNet.Api.Services
{
    public interface ICustomerContextService
    {
        Task<Result<CustomerContext>> Get(int customerId);
    }
}
