namespace TraderForge.Domain.Services;

public interface IIdentityService
{
    Task RegisterNewAccountAsync(string newUserId,string email, string password);
    Task<string> GetValidatedTokenAsync(string email, string password);
    

}