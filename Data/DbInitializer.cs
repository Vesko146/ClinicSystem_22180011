using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using ClinicSystem_22180011.Models;

namespace ClinicSystem_22180011.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>(); // Тук трябва да е твоят модел User

            // 1. СЪЗДАВАНЕ НА РОЛИ
            string[] roleNames = { "Admin", "Doctor", "Patient" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. СЪЗДАВАНЕ НА АДМИН
            var adminEmail = "admin@admin.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Правим обекта и му попълваме задължителните полета ръчно за всеки случай
                var newAdmin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                // ВАЖНО: Използваме метода, който приема паролата като втори параметър!
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
                else
                {
                    // Ако има грешка, ще я видим в конзолата
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"[SEED ERROR]: {errors}");
                }
            }
        }
    }
}