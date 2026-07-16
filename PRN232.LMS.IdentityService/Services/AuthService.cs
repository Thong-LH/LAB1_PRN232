using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.IdentityService.Data;
using PRN232.LMS.IdentityService.Entities;
using PRN232.LMS.IdentityService.Models;

namespace PRN232.LMS.IdentityService.Services;

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(IdentityDbContext dbContext, IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthTokenResponse?> LoginAsync(LoginRequest login)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == login.Username.Trim());

        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await IssueTokensAsync(user);
    }

    public async Task<AuthTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (existingToken?.User is null || !existingToken.IsActive)
        {
            return null;
        }

        var user = existingToken.User;
        var newRefreshToken = GenerateRefreshToken();
        newRefreshToken.UserId = user.UserId;
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        _dbContext.RefreshTokens.Update(existingToken);
        await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        return new AuthTokenResponse
        {
            AccessToken = GenerateAccessToken(user),
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = _jwtOptions.ExpiresInMinutes * 60
        };
    }

    private async Task<AuthTokenResponse> IssueTokensAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        refreshToken.UserId = user.UserId;

        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new AuthTokenResponse
        {
            AccessToken = GenerateAccessToken(user),
            RefreshToken = refreshToken.Token,
            ExpiresIn = _jwtOptions.ExpiresInMinutes * 60
        };
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiresInDays)
        };
    }
}
