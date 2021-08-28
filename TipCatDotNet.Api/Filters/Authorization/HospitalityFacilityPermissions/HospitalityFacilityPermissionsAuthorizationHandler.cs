using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions
{
    public class HospitalityFacilityPermissionsAuthorizationHandler : AuthorizationHandler<HospitalityFacilityPermissionsAuthorizationRequirement>
    {
        public HospitalityFacilityPermissionsAuthorizationHandler(ILoggerFactory loggerFactory, IEmployeeContextService employeeContextService,
            IPermissionChecker permissionChecker)
        {
            _logger = loggerFactory.CreateLogger<HospitalityFacilityPermissionsAuthorizationHandler>();
            _permissionChecker = permissionChecker;
            _employeeContextService = employeeContextService;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            HospitalityFacilityPermissionsAuthorizationRequirement requirement)
        {
            var (_, isEmployeeFailure, employee, employeeError) = await _employeeContextService.GetInfo();
            if (isEmployeeFailure)
            {
                _logger.LogEmployeeAuthorizationFailure(employeeError);
                context.Fail();
                return;
            }

            var (_, isPermissionFailure, permissionError) = await _permissionChecker.CheckEmployeePermissions(employee, requirement.Permissions);
            if (isPermissionFailure)
            {
                _logger.LogEmployeeAuthorizationFailure(permissionError);
                context.Fail();
                return;
            }

            _logger.LogEmployeeAuthorizationSuccess(employee.Email, requirement.Permissions.ToString());
            context.Succeed(requirement);
        }


        private readonly ILogger<HospitalityFacilityPermissionsAuthorizationHandler> _logger;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IEmployeeContextService _employeeContextService;
    }
}
