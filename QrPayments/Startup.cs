using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using TipCatDotNet.Data;
using TipCatDotNet.Infrastructure;

namespace TipCatDotNet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CustomerDbContext>(options =>
            {
                options.UseSqlite("Data Source=sqlitedemo.db");
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            });

            // https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/4-WebApp-your-API/4-2-B2C
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                    {
                        Configuration.Bind("AzureAdB2C", options);

                        options.TokenValidationParameters.NameClaimType = "name";
                    },
                    options => { Configuration.Bind("AzureAdB2C", options); });
            
            services.AddServices();
            services.AddControllers();
            services.AddHealthChecks()
                //.AddDbContextCheck<T>()
                //.AddRedis(EnvironmentVariableHelper.Get("Redis:Endpoint", Configuration))
                .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "QrPayments", Version = "v1" });
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsLocal())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QrPayments v1"));
            }

            app.UseHttpsRedirection();
            app.UseHsts();

            app.UseHealthChecks("/health");
            app.UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
        

        public IConfiguration Configuration { get; }
    }
}
