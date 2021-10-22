using System;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using TipCatDotNet.Api.Filters.Authorization.HospitalityFacilityPermissions;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Services.Auth;
using TipCatDotNet.Api.Services.HospitalityFacilities;
using TipCatDotNet.Api.Services.Payments;
using TipCatDotNet.Api.Services.Permissions;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IUserManagementClient, Auth0UserManagementClient>(c =>
            {
                c.BaseAddress = new Uri(configuration["Auth0:Audience"]);
                c.DefaultRequestHeaders.Add("content-type", "application/json");
            });

            return services.AddHttpClient();
        }


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
            var auth0Options = vaultClient.Get(configuration["Auth0:Options"]).GetAwaiter().GetResult();
            services.Configure<Auth0ManagementApiOptions>(o =>
            {
                o.ClientId = auth0Options["clientId"];
                o.ClientSecret = auth0Options["clientSecret"];
                o.ConnectionId = "";
                o.Domain = new Uri(configuration["Auth0:Domain"]);
            });

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
