using TraderForge.Domain.Common;

namespace TraderForge.Domain.Services;

public interface IIdentityService
{
    Task<Result> RegisterNewAccountAsync(string newUserId, string email, string password);
    Task<ResultGeneric<string>> GetValidatedTokenAsync(string email, string password);
    Task<Result> DeleteAccountAsync(string userId);
}
