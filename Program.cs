using Asm_GD1.Data;
using Asm_GD1.Middleware;
using Asm_GD1.Models;
using Asm_GD1.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers with views
builder.Services.AddControllersWithViews();

// Add API controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

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

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Add JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Food Order API",
        Version = "v1",
        Description = "REST API for Food Order Application",
        Contact = new OpenApiContact
        {
            Name = "Food Order Team",
            Email = "support@foodorder.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
var corsSettings = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsSettings)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
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
else
{
    // Enable Swagger in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Food Order API v1");
        options.RoutePrefix = "swagger";
    });
}

// Use error handler middleware for API
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Map MVC routes
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