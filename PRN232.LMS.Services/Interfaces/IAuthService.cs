using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IAuthService
{
    Task<AuthTokenBusinessModel?> LoginAsync(LoginBusinessModel login);

    Task<AuthTokenBusinessModel?> RefreshTokenAsync(RefreshTokenBusinessModel request);
}
