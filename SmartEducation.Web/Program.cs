using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartEducation.Application.Constants;
using SmartEducation.Domain.Entities;
using SmartEducation.Persistence.Contexts;
using SmartEducation.Persistence.Seeders;
using SmartEducation.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
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

// Add this line to register your persistence layer repositories
builder.Services.AddPersistenceServices();

// Register MediatR and tell it to look for Handlers inside the Application assembly
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SmartEducation.Application.Features.Subjects.Queries.GetSubjectsQuery).Assembly));
// Add Authorization Policies

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        Permissions.ManageUsers,
        policy => policy.RequireRole(Roles.Admin));

    options.AddPolicy(
        Permissions.ManageSubjects,
        policy => policy.RequireRole(Roles.Admin));

    options.AddPolicy(
        Permissions.ManageExams,
        policy => policy.RequireRole(Roles.Teacher));

    options.AddPolicy(
        Permissions.ManageAttendance,
        policy => policy.RequireRole(Roles.Teacher));

    options.AddPolicy(
        Permissions.ViewAnalytics,
        policy => policy.RequireRole(Roles.Admin));

    options.AddPolicy(
        Permissions.ManageLessonPlans,
        policy => policy.RequireRole(Roles.Teacher));
});

var app = builder.Build();
// Run Seeders Automatically

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<ApplicationRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    await RoleSeeder.SeedAsync(roleManager);

    await AdminSeeder.SeedAsync(userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
// Area Routes
// teacher area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Teacher}/{action=Index}/{id?}"
);
// admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
// default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
