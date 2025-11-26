using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageFood", policy =>
        policy.RequireClaim("CanManageFood", "true"));
    options.AddPolicy("CanManageUser", policy =>
        policy.RequireClaim("CanManageUser", "true"));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<AppDbContext>();
        await SeedDataAsync(userManager, roleManager, context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

async Task SeedDataAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
{
    string[] roles = { "AdminIT", "FoodAdmin", "UserAdmin", "Staff", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ✅ SuperAdmin (AdminIT)
    if (await userManager.FindByEmailAsync("adminit@gmail.com") == null)
    {
        var admin = new ApplicationUser
        {
            UserName = "adminit@gmail. com",
            Email = "adminit@gmail.com",
            FullName = "Super Administrator",
            EmailConfirmed = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(admin, "AdminIT@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "AdminIT");
        }
    }

    // ✅ Food Admin
    if (await userManager.FindByEmailAsync("foodadmin@gmail.com") == null)
    {
        var foodAdmin = new ApplicationUser
        {
            UserName = "foodadmin@gmail.com",
            Email = "foodadmin@gmail.com",
            FullName = "Quản lý Món ăn",
            EmailConfirmed = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(foodAdmin, "FoodAdmin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(foodAdmin, "FoodAdmin");
        }
    }

    // ✅ User Admin
    if (await userManager.FindByEmailAsync("useradmin@gmail.com") == null)
    {
        var userAdmin = new ApplicationUser
        {
            UserName = "useradmin@gmail.com",
            Email = "useradmin@gmail.com",
            FullName = "Quản lý User",
            EmailConfirmed = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(userAdmin, "UserAdmin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(userAdmin, "UserAdmin");
        }
    }

    // ✅ STAFF 1 - Nhân viên bán hàng
    if (await userManager.FindByEmailAsync("staff@gmail.com") == null)
    {
        var staff = new ApplicationUser
        {
            UserName = "staff@gmail. com",
            Email = "staff@gmail.com",
            FullName = "Nhân viên bán hàng",
            PhoneNumber = "0123456789",
            EmailConfirmed = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(staff, "Staff@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(staff, "Staff");
        }
    }

    // ✅ STAFF 2 - Nhân viên ca 2
    if (await userManager.FindByEmailAsync("staff2@gmail.com") == null)
    {
        var staff2 = new ApplicationUser
        {
            UserName = "staff2@gmail.com",
            Email = "staff2@gmail.com",
            FullName = "Nguyễn Thị B",
            PhoneNumber = "0987654321",
            EmailConfirmed = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(staff2, "Staff@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(staff2, "Staff");
        }
    }

    // ✅ Customer Demo
    if (await userManager.FindByEmailAsync("customer@gmail.com") == null)
    {
        var customer = new ApplicationUser
        {
            UserName = "customer@gmail.com",
            Email = "customer@gmail.com",
            FullName = "Khách hàng demo",
            PhoneNumber = "0369852147",
            EmailConfirmed = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var result = await userManager.CreateAsync(customer, "Customer@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(customer, "Customer");
        }
    }
}