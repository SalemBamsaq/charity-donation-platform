using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Application.Services;
using CharityDonationPlatform.Domain.Enums;
using CharityDonationPlatform.Domain.Models;
using CharityDonationPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity for .NET 8.0
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Set to true in production
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false; // Simplified for development
    options.Password.RequiredLength = 6; // Simplified for development
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
});

// Register application services
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();

// Configure Stripe (if keys are provided)
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrEmpty(stripeSecretKey))
{
    StripeConfiguration.ApiKey = stripeSecretKey;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed initial data (admin user, roles, etc.)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created and apply migrations
        await context.Database.EnsureCreatedAsync();

        // Seed initial data
        await SeedData(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Seed data method
async Task SeedData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Create roles if they don't exist
    if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

    if (!await roleManager.RoleExistsAsync(UserRoles.Donor))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Donor));

    if (!await roleManager.RoleExistsAsync(UserRoles.Staff))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Staff));

    // Create admin user if it doesn't exist
    if (await userManager.FindByEmailAsync("admin@example.com") == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            ProfilePictureUrl = "/images/default-profile.png"
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
        }
    }

    // Create sample staff user
    if (await userManager.FindByEmailAsync("staff@example.com") == null)
    {
        var staffUser = new ApplicationUser
        {
            UserName = "staff@example.com",
            Email = "staff@example.com",
            EmailConfirmed = true,
            FirstName = "Staff",
            LastName = "Member",
            ProfilePictureUrl = "/images/default-profile.png"
        };

        var result = await userManager.CreateAsync(staffUser, "Staff@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(staffUser, UserRoles.Staff);
        }
    }

    // Add sample campaigns if none exist
    if (!context.Campaigns.Any())
    {
        var admin = await userManager.FindByEmailAsync("admin@example.com");

        if (admin != null)
        {
            var sampleCampaigns = new List<Campaign>
            {
                new Campaign
                {
                    Title = "Clean Water Initiative",
                    Description = "Help us provide clean water to communities in need. This campaign aims to build wells and water purification systems in areas lacking access to clean drinking water. Your donation will directly fund the construction of water infrastructure that will serve hundreds of families for years to come.",
                    TargetAmount = 10000,
                    AmountRaised = 2500,
                    StartDate = DateTime.UtcNow.Date.AddDays(-30),
                    EndDate = DateTime.UtcNow.Date.AddDays(60),
                    ImageUrl = "/images/sample/water.jpg",
                    IsActive = true,
                    CreatedById = admin.Id,
                    Created = DateTime.UtcNow.AddDays(-30)
                },
                new Campaign
                {
                    Title = "Education for All",
                    Description = "Support our mission to provide quality education to underprivileged children. Funds will be used to build schools, provide learning materials, and train teachers. Education is the key to breaking the cycle of poverty and creating opportunities for future generations.",
                    TargetAmount = 15000,
                    AmountRaised = 7500,
                    StartDate = DateTime.UtcNow.Date.AddDays(-45),
                    EndDate = DateTime.UtcNow.Date.AddDays(45),
                    ImageUrl = "/images/sample/education.jpg",
                    IsActive = true,
                    CreatedById = admin.Id,
                    Created = DateTime.UtcNow.AddDays(-45)
                },
                new Campaign
                {
                    Title = "Wildlife Conservation",
                    Description = "Join our efforts to protect endangered species and their habitats. Your donation will fund conservation programs, anti-poaching initiatives, and wildlife sanctuaries. Together, we can preserve biodiversity for future generations and maintain the delicate balance of our ecosystems.",
                    TargetAmount = 20000,
                    AmountRaised = 5000,
                    StartDate = DateTime.UtcNow.Date.AddDays(-15),
                    EndDate = DateTime.UtcNow.Date.AddDays(75),
                    ImageUrl = "/images/sample/wildlife.jpg",
                    IsActive = true,
                    CreatedById = admin.Id,
                    Created = DateTime.UtcNow.AddDays(-15)
                }
            };

            await context.Campaigns.AddRangeAsync(sampleCampaigns);
            await context.SaveChangesAsync();
        }
    }
}