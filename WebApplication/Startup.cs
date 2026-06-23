using System;
using System.IO;
using DbHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quotes.DataModel;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            // env
            new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // env
            services.Configure<EnvironmentConfig>(Configuration);

            services.AddControllersWithViews();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("IdentityDbContext")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            { 
                options.LoginPath = $"/Account/Login";
                options.LogoutPath = $"/Account/Logout";
                options.AccessDeniedPath = $"/Account/AccessDenied";
            });

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddDbContext<QuotesDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("QuotesDbContext")));

            services.AddScoped<IApplicationDbInitialization, ApplicationDbInitialization>();
            
        }

        // Create the SQLite databases on first run (schema via EnsureCreated; the
        // Identity DB also seeds the admin user/role from AdminUserSettings config).
        private static void InitializeDatabases(IApplicationBuilder app)
        {
            Directory.CreateDirectory("data");

            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            services.GetRequiredService<QuotesDbContext>().Database.EnsureCreated();
            services.GetRequiredService<IApplicationDbInitialization>().Init();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<EnvironmentConfig> envConf)
        {
            InitializeDatabases(app);

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

            // custom
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            
            // custom
            var settingsPathBase = envConf.Value.QuotesPathBase.AsNullIfEmpty() ?? Configuration.GetSection("Settings")["PathBase"];
            Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"Using PathBase: {settingsPathBase}");
            app.UsePathBase(settingsPathBase);
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

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