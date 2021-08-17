using Microsoft.Extensions.DependencyInjection;
using TipCatDotNet.Services;

namespace TipCatDotNet.Infrastructure
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<ICompanyService, CompanyService>();
            services.AddTransient<ICustomerContextService, CustomerContextService>();
            services.AddTransient<ICustomerService, CustomerService>();
            
            return services;
        }
    }
}
