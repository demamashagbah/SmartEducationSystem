using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartEducation.Application.Constants;
using SmartEducation.Domain.Entities;
using SmartEducation.Infrastructure;
using SmartEducation.Persistence;
using SmartEducation.Persistence.Contexts;
using SmartEducation.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddPersistenceServices();

builder.Services.AddInfrastructureServices();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(SmartEducation.Application.Features.Subjects.Queries.GetSubjectsQuery).Assembly));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Permissions.ManageUsers, policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy(Permissions.ManageSubjects, policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy(Permissions.ManageExams, policy => policy.RequireRole(Roles.Teacher, Roles.Admin));
    options.AddPolicy(Permissions.ManageAttendance, policy => policy.RequireRole(Roles.Teacher, Roles.Admin));
    options.AddPolicy(Permissions.ViewAnalytics, policy => policy.RequireRole(Roles.Admin, Roles.Teacher));
    options.AddPolicy(Permissions.ManageLessonPlans, policy => policy.RequireRole(Roles.Teacher));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await RoleSeeder.SeedAsync(roleManager);
    await AdminSeeder.SeedAsync(userManager);

    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DataSeeder.SeedAsync(dbContext);
    await ProfileSeeder.SeedMissingProfilesAsync(userManager, dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
