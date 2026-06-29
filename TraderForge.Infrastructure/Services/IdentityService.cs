using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TraderForge.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

using TraderForge.Domain.Common;

namespace TraderForge.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Account> _userManager;
    private readonly IConfiguration _jwtConfiguration;

    public IdentityService(UserManager<Account> userManager, IConfiguration jwtConfiguration)
    {
        _userManager = userManager;
        _jwtConfiguration = jwtConfiguration;
    }

    public async Task<Result> RegisterNewAccountAsync(string newUserId, string email, string password)
    {
        var newApplicationUser = new Account()
        {
            Id = newUserId,
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(newApplicationUser, password);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Unknown registration error";
            return Result.Failure(errorMessage);
        }
        
        await _userManager.AddClaimAsync(newApplicationUser, new Claim(ClaimTypes.Role, "Trader"));
        return Result.Success();
    }

    private void EnsureSuccessOrThrow(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Unknown registration error";
            throw new Exception($"User registration failed: {errorMessage}");
        }
    }

    public async Task<Result> DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Account not found.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Unknown deletion error";
            return Result.Failure(errorMessage);
        }

        return Result.Success();
    }

    public async Task<ResultGeneric<string>> GetValidatedTokenAsync(string email, string password)
    {
        Account? user = await GetApplicationUserByEmailAsync(email);
        if (user == null || !await IsUserValidatedAsync(user, password))
        {
            return ResultGeneric<string>.Failure("Invalid Credentials");
        }
        
        string token = await GenerateJwtTokenForUserAsync(user);
        return ResultGeneric<string>.Success(token);
    }

    private async Task<Account?> GetApplicationUserByEmailAsync(string email)
    {
        try
        {
            return await _userManager.FindByEmailAsync(email);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    private async Task<bool> IsUserValidatedAsync(Account user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    private async Task<string> GenerateJwtTokenForUserAsync(Account user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(await GenerateSecurityTokenDescriptorAsync(user));

        return tokenHandler.WriteToken(token);
    }

    private async Task<SecurityTokenDescriptor> GenerateSecurityTokenDescriptorAsync(Account user)
    {
        string issuer = _jwtConfiguration["JwtSettings:Issuer"] ?? throw new Exception("JWT Issuer is missing!");
        string audience = _jwtConfiguration["JwtSettings:Audience"] ?? throw new Exception("JWT Audience is missing!");

        List<Claim> claims = await GetUserClaimsAsync(user);    
        
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), 
            Expires = DateTime.UtcNow.AddHours(2), 
            Issuer = issuer,                      
            Audience = audience,                 
            SigningCredentials = RetrieveSigningCredentials()
        };
    }

    private async Task<List<Claim>> GetUserClaimsAsync(Account user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };
        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return claims;

    }

    private SigningCredentials RetrieveSigningCredentials()
    {
        string secret = _jwtConfiguration["JwtSettings:Secret"] ?? throw new Exception("JWT Secret is missing!");
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
    }

    


}