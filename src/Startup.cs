using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quotes.Data;
using Quotes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DbHelpers;
using Microsoft.AspNetCore.HttpOverrides;

namespace Quotes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private readonly IConfiguration configuration;

        private string PathBase
        {
            get
            {
                return configuration["pathBase"];
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("IdentityDbContext")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            { 
                options.LoginPath = $"{PathBase}/Account/Login";
                options.LogoutPath = $"{PathBase}/Account/Logout";
                options.AccessDeniedPath = $"{PathBase}/Account/AccessDenied";
            });

            services.AddMvc();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddDbContext<QuotesDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("QuotesDbContext")));

            services.AddScoped<IApplicationDbInitialization, ApplicationDbInitialization>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            app.UsePathBase(PathBase);
            app.UseStaticFiles(PathBase);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Quotes}/{action=Index}");
            });

            serviceProvider.GetService<IApplicationDbInitialization>().Init();
        }
    }
}
