using Microsoft.AspNetCore.Identity;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;

namespace SmartEducation.Persistence.Seeders
{
    public static class ProfileSeeder
    {
        public static async Task SeedMissingProfilesAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            var teacherUsers = await userManager.GetUsersInRoleAsync("Teacher");
            foreach (var user in teacherUsers)
            {
                var hasProfile = context.TeacherProfiles.Any(tp => tp.UserId == user.Id && !tp.IsDeleted);
                if (!hasProfile)
                {
                    context.TeacherProfiles.Add(new TeacherProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        EmployeeNumber = $"EMP-{user.Id.ToString()[..8].ToUpper()}"
                    });
                }
            }

            var studentUsers = await userManager.GetUsersInRoleAsync("Student");
            var firstClassRoom = context.ClassRooms.FirstOrDefault(c => !c.IsDeleted);
            if (firstClassRoom != null)
            {
                foreach (var user in studentUsers)
                {
                    var hasProfile = context.StudentProfiles.Any(sp => sp.UserId == user.Id && !sp.IsDeleted);
                    if (!hasProfile)
                    {
                        context.StudentProfiles.Add(new StudentProfile
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            ClassRoomId = firstClassRoom.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
