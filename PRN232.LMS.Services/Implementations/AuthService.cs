using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Options;

namespace PRN232.LMS.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(IUserRepository userRepository, IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthTokenBusinessModel?> LoginAsync(LoginBusinessModel login)
    {
        var user = await _userRepository.GetByUsernameAsync(login.Username.Trim());

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

    public async Task<AuthTokenBusinessModel?> RefreshTokenAsync(RefreshTokenBusinessModel request)
    {
        var existingToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);

        if (existingToken?.User is null || !existingToken.IsActive)
        {
            return null;
        }

        var user = existingToken.User;
        var newRefreshToken = GenerateRefreshToken();
        newRefreshToken.UserId = user.UserId;
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        _userRepository.UpdateRefreshToken(existingToken);
        await _userRepository.AddRefreshTokenAsync(newRefreshToken);
        await _userRepository.SaveChangesAsync();

        return new AuthTokenBusinessModel
        {
            AccessToken = GenerateAccessToken(user),
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = _jwtOptions.ExpiresInMinutes * 60
        };
    }

    private async Task<AuthTokenBusinessModel> IssueTokensAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        refreshToken.UserId = user.UserId;

        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new AuthTokenBusinessModel
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
