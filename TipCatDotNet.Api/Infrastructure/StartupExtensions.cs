using Microsoft.Extensions.DependencyInjection;
//using TipCatDotNet.Api.Services;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            //services.AddTransient<ICompanyService, CompanyService>();
            //services.AddTransient<ICustomerContextService, CustomerContextService>();
            //services.AddTransient<ICustomerService, CustomerService>();
            
            return services;
        }
    }
}
