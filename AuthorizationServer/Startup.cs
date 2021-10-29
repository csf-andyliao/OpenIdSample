using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthorizationServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
            services.AddControllersWithViews();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                    {
                        options.LoginPath = "/account/index";
                    });


            services.AddOpenIddict()
                    .AddCore(options =>
                    {
                        options.UseEntityFrameworkCore()
                               .UseDbContext<DbContext>();
                    })
                    .AddServer(options =>
                    {
                        options.AllowAuthorizationCodeFlow();
                        options.AllowClientCredentialsFlow();
                        options.AllowRefreshTokenFlow();

                        options.SetAuthorizationEndpointUris("/connect/authorize");
                        options.SetTokenEndpointUris("/connect/token");
                        options.SetIntrospectionEndpointUris("/connect/introspect");

                        options.AddDevelopmentEncryptionCertificate()
                               .AddDevelopmentSigningCertificate()
                               .DisableAccessTokenEncryption();

                        options.RegisterScopes("weather_api_1", "weather_api_2");

                        options.UseAspNetCore()
                               .EnableTokenEndpointPassthrough()
                               .EnableAuthorizationEndpointPassthrough();
                    })
                    .AddValidation(options =>
                    {
                        options.UseLocalServer();
                        options.UseAspNetCore();
                    });

            services.AddDbContext<DbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("AuthorizationServer"));
                options.UseOpenIddict();
            });

            services.AddHostedService<Worker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
