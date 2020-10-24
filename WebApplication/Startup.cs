using System;
using System.IO;
using DbHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quotes.DataModel;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private string PathBase => Configuration["pathBase"];

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            CheckDatabases();
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("IdentityDbContext")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            { 
                options.LoginPath = $"{PathBase}/Account/Login";
                options.LogoutPath = $"{PathBase}/Account/Logout";
                options.AccessDeniedPath = $"{PathBase}/Account/AccessDenied";
            });

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddDbContext<QuotesDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("QuotesDbContext")));

            services.AddScoped<IApplicationDbInitialization, ApplicationDbInitialization>();
            
        }

        private void CheckDatabases()
        {
            var quoteDbPath = Path.Combine("data", "quote.db");
            var appDbPath = Path.Combine("data", "app.db");
            var quoteDbDefaultPath = Path.Combine("data-default", "quote-default.db");
            var appDbDefaultPath = Path.Combine("data-default", "app-default.db");
            
            if (!File.Exists(quoteDbPath)) File.Copy(quoteDbDefaultPath, quoteDbPath);
            if (!File.Exists(appDbPath)) File.Copy(appDbDefaultPath, appDbPath);
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
            
            // custom
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();

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