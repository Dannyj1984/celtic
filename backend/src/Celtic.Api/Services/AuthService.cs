using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CelticDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        CelticDbContext db,
        IConfiguration config)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwtToken(user);

        return new LoginResponse(
            Token: token,
            UserId: user.Id,
            Email: user.Email!,
            FullName: user.FullName,
            Role: user.Role
        );
    }

    public async Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Phone = request.Phone ?? string.Empty,
            Role = request.Role
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create account: {errors}");
        }

        return new CreateAccountResponse(
            UserId: user.Id,
            Email: user.Email!,
            FullName: user.FullName,
            Role: user.Role
        );
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var children = await _db.PlayerParents
            .Where(pp => pp.UserId == userId)
            .Include(pp => pp.Player)
            .Select(pp => new LinkedPlayerDto(
                pp.PlayerId,
                pp.Player.FirstName,
                pp.Player.LastName,
                pp.Relationship,
                pp.Player.SubscriptionStatus
            ))
            .ToListAsync();

        return new UserInfoResponse(
            UserId: user.Id,
            Email: user.Email!,
            FullName: user.FullName,
            Phone: user.Phone,
            Role: user.Role,
            Children: children
        );
    }

    public async Task<List<UserInfoResponse>> GetAllParentsAsync()
    {
        var users = await _userManager.Users
            .Where(u => u.Role == "Parent")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var parentIds = users.Select(u => u.Id).ToList();
        var allChildren = await _db.PlayerParents
            .Where(pp => parentIds.Contains(pp.UserId))
            .Include(pp => pp.Player)
            .ToListAsync();

        var responses = users.Select(u => new UserInfoResponse(
            UserId: u.Id,
            Email: u.Email!,
            FullName: u.FullName,
            Phone: u.Phone,
            Role: u.Role,
            Children: allChildren
                .Where(pp => pp.UserId == u.Id)
                .Select(pp => new LinkedPlayerDto(
                    pp.PlayerId,
                    pp.Player.FirstName,
                    pp.Player.LastName,
                    pp.Relationship,
                    pp.Player.SubscriptionStatus
                ))
                .ToList()
        )).ToList();

        return responses;
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to change password: {errors}");
        }
    }

    public async Task LinkPlayerToParentAsync(LinkPlayerRequest request)
    {
        var existing = await _db.PlayerParents
            .FirstOrDefaultAsync(pp => pp.PlayerId == request.PlayerId && pp.UserId == request.UserId);

        if (existing != null) return;

        var link = new PlayerParent
        {
            PlayerId = request.PlayerId,
            UserId = request.UserId,
            Relationship = request.Relationship
        };

        _db.PlayerParents.Add(link);
        await _db.SaveChangesAsync();
    }

    public async Task AdminResetPasswordAsync(AdminResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        // Force reset by removing and re-adding password
        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
             var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
             throw new InvalidOperationException($"Failed to remove current password: {errors}");
        }

        var addResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to set new password: {errors}");
        }
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "celtic-api",
            audience: _config["Jwt:Audience"] ?? "celtic-app",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
