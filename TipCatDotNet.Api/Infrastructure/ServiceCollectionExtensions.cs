using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions;
using TipCatDotNet.Api.Services.Graph;
using TipCatDotNet.Api.Services.HospitalityFacilities;

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


        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IAuthorizationHandler, MemberPermissionsAuthorizationHandler>();

            services.AddTransient<IMicrosoftGraphClient, MicrosoftGraphClient>();

            services.AddTransient<IMemberContextService, MemberContextService>();
            services.AddTransient<IPermissionChecker, PermissionChecker>();

            services.AddTransient<IMemberService, MemberService>();
            services.AddTransient<IAccountService, AccountService>();
            
            return services;
        }
    }
}
