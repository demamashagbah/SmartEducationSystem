using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.Constants;
using SmartEducation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Persistence.Seeders
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
       UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@smarteducation.com";

            var existingAdmin =
                await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin != null)
                return;

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                admin,
                "Admin@123");

            // IMPORTANT
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }

                return;
            }

            var roleResult = await userManager.AddToRoleAsync(
                admin,
                Roles.Admin);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }
    }
}
            
        
