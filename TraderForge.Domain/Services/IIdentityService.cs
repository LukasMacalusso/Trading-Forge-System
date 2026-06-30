using TraderForge.Domain.Common;

namespace TraderForge.Domain.Services;

public interface IIdentityService
{
    Task<Result> RegisterNewAccountAsync(string newUserId, string email, string password);
    Task<ResultGeneric<TokenResponse>> GetValidatedTokenAsync(string email, string password);
    Task<ResultGeneric<TokenResponse>> RefreshTokenAsync(string token, string refreshToken);
    Task<Result> DeleteAccountAsync(string userId);
}
