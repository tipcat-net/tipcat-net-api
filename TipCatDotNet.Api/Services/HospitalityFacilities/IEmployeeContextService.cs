using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IEmployeeContextService
    {
        ValueTask<Result<EmployeeContext>> GetInfo();
    }
}
