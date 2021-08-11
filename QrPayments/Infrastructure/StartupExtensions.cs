using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QrPayments.Services;

namespace QrPayments.Infrastructure
{
    public static class StartupExtensions
    {
        public static AuthenticationBuilder AddAuth(this IServiceCollection services, IConfiguration configuration)
            => services.AddAuthentication(o =>
                {
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme/* + "," + JwtBearerDefaults.AuthenticationScheme*/;
                    o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
                    o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
                })
                /*.AddJwtBearer()*/
                .AddCookie()
                .AddGoogleOpenIdConnect(o =>
                {
                    o.ClientId = configuration.GetValue<string>("OAuth:web:client_id");
                    o.ClientSecret = configuration.GetValue<string>("OAuth:web:client_secret");
                });


        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<ICompanyService, CompanyService>();
            services.AddTransient<ICustomerContextService, CustomerContextService>();
            services.AddTransient<ICustomerService, CustomerService>();

            services.AddControllers();
            services.AddControllersWithViews();
            
            return services;
        }
    }
}
