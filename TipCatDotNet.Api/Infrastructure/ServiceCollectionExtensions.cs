using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Services.Payments;
using TipCatDotNet.Api.Services.Permissions;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoftGraphClient(this IServiceCollection services, IConfiguration configuration)
            => services.AddTransient(_ =>
            {
                var confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(configuration["AzureB2CUserManagement:AppId"])
                    .WithTenantId(configuration["AzureB2CUserManagement:TenantId"])
                    .WithClientSecret(configuration["AzureB2CUserManagement:ClientSecret"])
                    .Build();

                var clientCredentialProvider = new ClientCredentialProvider(confidentialClientApplication);

                // https://docs.microsoft.com/ru-ru/azure/active-directory-b2c/microsoft-graph-get-started?tabs=app-reg-ga
                return new GraphServiceClient(clientCredentialProvider);
            });


        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
        {
            

            return services;
        }


        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IAuthorizationHandler, MemberPermissionsAuthorizationHandler>();

            services.AddTransient<IUserManagementClient, Auth0UserManagementClient>();

            services.AddTransient<IMemberContextCacheService, MemberContextCacheService>();
            services.AddTransient<IMemberContextService, MemberContextService>();
            services.AddTransient<IPermissionChecker, PermissionChecker>();

            services.AddTransient<IQrCodeGenerator, QrCodeGenerator>();

            services.AddTransient<IInvitationService, InvitationService>();
            services.AddTransient<IFacilityService, FacilityService>();
            services.AddTransient<IMemberService, MemberService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IPaymentService, PaymentService>();

            return services;
        }
    }
}
