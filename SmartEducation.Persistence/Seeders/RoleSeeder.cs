using Microsoft.AspNetCore.Identity;
using SmartEducation.Application.Constants;
using SmartEducation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartEducation.Persistence.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
        {

            string[] roles =
            {
            Roles.Admin,
            Roles.Teacher,
            Roles.Student,
            Roles.Parent
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new ApplicationRole
                        {
                            Name = role
                        });
                }
            }
        }
    }
}
 
    
  