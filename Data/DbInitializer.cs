using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // This fixes the IServiceProvider error
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicSystem_22180011.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

           
            string[] roleNames = { "Admin", "Doctor", "Patient" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles if they don't exist
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}