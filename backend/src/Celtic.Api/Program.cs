using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Celtic.Api.Data;
using Celtic.Api.Models;
using Celtic.Api.Services;

// Prevent JWT handler from remapping claim types
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<CelticDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<CelticDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CelticDevKeyThatIsAtLeast32Characters!";
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "celtic-api",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "celtic-app",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate and seed admin on startup (dev only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CelticDbContext>();

    // Only migrate if using a relational database (skip for InMemory test provider)
    if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    // Seed admin user if none exists
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (!await userManager.Users.AnyAsync(u => u.Role == "Admin"))
    {
        var admin = new ApplicationUser
        {
            UserName = "admin@celtic.app",
            Email = "admin@celtic.app",
            FullName = "Team Admin",
            Role = "Admin"
        };
        await userManager.CreateAsync(admin, "Admin123!");
    }
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
