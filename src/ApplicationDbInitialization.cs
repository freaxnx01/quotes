using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbHelpers
{
    public class ApplicationDbInitialization : IApplicationDbInitialization
    {
        private readonly IServiceProvider serviceProvider;

        private const string ConfSection = "AdminUserSettings";
        private const string ConfValueRoleName = "RoleName";
        private const string ConfValueEmail = "Email";
        private const string ConfValuePwd = "Pwd";

        public ApplicationDbInitialization(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Init()
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            bool created = context.Database.EnsureCreated();
            if (created)
            {
                CreateUserAndRole().Wait();
            }
        }

        private Dictionary<string, string> GetConfigurationValues()
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            //var configurationSection = configuration.GetSection(ConfSection);
            //if (configurationSection.Value == null)
            //{
            //    throw new ApplicationException($"Configuration section '{ConfSection}' is missing.");
            //}

            return new Dictionary<string, string>() {
                { ConfValueRoleName, configuration[$"{ConfSection}:{ConfValueRoleName}"] },
                { ConfValueEmail, configuration[$"{ConfSection}:{ConfValueEmail}"] },
                { ConfValuePwd, configuration[$"{ConfSection}:{ConfValuePwd}"] }
            };
        }

        private async Task CreateUserAndRole()
        {
            var configurationValues = GetConfigurationValues();

            //adding custom roles
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            IdentityResult roleResult;

            string adminRoleName = configurationValues[ConfValueRoleName];
            var roleExist = await roleManager.RoleExistsAsync(adminRoleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            //creating a super user who could maintain the web app
            string emailAddress = configurationValues[ConfValueEmail];
            var poweruser = new IdentityUser
            {
                UserName = emailAddress,
                Email = emailAddress
            };

            string pwd = configurationValues[ConfValuePwd];

            var user = await userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                var createPowerUser = await userManager.CreateAsync(poweruser, pwd);
                if (createPowerUser.Succeeded)
                {
                    //here we tie the new user to the "Admin" role 
                    await userManager.AddToRoleAsync(poweruser, adminRoleName);
                }
            }
        }
    }
}
