using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to config explicitly if DotNetEnv loads them globally
builder.Configuration.AddEnvironmentVariables();

// Database
builder.Services.AddDbContext<CelticDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<CelticDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured. Set Jwt:Key in appsettings or the JWT_KEY environment variable.");
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
    };
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHostedService<TrainingGeneratorService>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" };
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
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
    var adminUser = await userManager.FindByEmailAsync("admin@celtic.app");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@celtic.app",
            Email = "admin@celtic.app",
            FullName = "Team Admin",
            Role = "Admin"
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
    }
    else if (adminUser.Role != "Admin")
    {
        adminUser.Role = "Admin";
        await userManager.UpdateAsync(adminUser);
    }
    if (!await userManager.Users.AnyAsync(u => u.UserName == "dannyjebb@gmail.com"))
    {
        var user = new ApplicationUser
        {
            UserName = "dannyjebb@gmail.com",
            Email = "dannyjebb@gmail.com",
            FullName = "Danny Jebb",
            Role = "User"
        };
        await userManager.CreateAsync(user, "user123!");
    }

    // Seed ClubSettings if none exists
    if (!await db.ClubSettings.AnyAsync())
    {
        db.ClubSettings.Add(new ClubSettings
        {
            TrainingDay = DayOfWeek.Tuesday,
            TrainingStartTime = new TimeSpan(18, 0, 0),
            TrainingEndTime = new TimeSpan(19, 30, 0),
            TrainingLocation = "Standard Pitch 1",
            CoachWhatsAppNumber = "07000000000"
        });
        await db.SaveChangesAsync();
    }
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
