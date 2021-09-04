using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Services.HospitalityFacilities;

namespace TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions
{
    public class MemberPermissionsAuthorizationHandler : AuthorizationHandler<MemberPermissionsAuthorizationRequirement>
    {
        public MemberPermissionsAuthorizationHandler(ILoggerFactory loggerFactory, IMemberContextService memberContextService,
            IPermissionChecker permissionChecker)
        {
            _logger = loggerFactory.CreateLogger<MemberPermissionsAuthorizationHandler>();
            _permissionChecker = permissionChecker;
            _memberContextService = memberContextService;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            MemberPermissionsAuthorizationRequirement requirement)
        {
            var (_, isMemberFailure, member, memberError) = await _memberContextService.Get();
            if (isMemberFailure)
            {
                _logger.LogMemberAuthorizationFailure(memberError);
                context.Fail();
                return;
            }

            var (_, isPermissionFailure, permissionError) = await _permissionChecker.CheckMemberPermissions(member, requirement.Permissions);
            if (isPermissionFailure)
            {
                _logger.LogMemberAuthorizationFailure(permissionError);
                context.Fail();
                return;
            }

            _logger.LogMemberAuthorizationSuccess(member.Email, requirement.Permissions.ToString());
            context.Succeed(requirement);
        }


        private readonly ILogger<MemberPermissionsAuthorizationHandler> _logger;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IMemberContextService _memberContextService;
    }
}
