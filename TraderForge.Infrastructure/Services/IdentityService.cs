using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TraderForge.Domain.Services;
using TraderForge.Domain.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Cryptography;
using TraderForge.Domain.Common;

namespace TraderForge.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Account> _userManager;
    private readonly IConfiguration _jwtConfiguration;
    private readonly ITraderRepository _traderRepository;

    public IdentityService(UserManager<Account> userManager, IConfiguration jwtConfiguration, ITraderRepository traderRepository)
    {
        _userManager = userManager;
        _jwtConfiguration = jwtConfiguration;
        _traderRepository = traderRepository;
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

    public async Task<ResultGeneric<TokenResponse>> GetValidatedTokenAsync(string email, string password)
    {
        Account? user = await GetApplicationUserByEmailAsync(email);
        if (user == null || !await IsUserValidatedAsync(user, password))
        {
            return ResultGeneric<TokenResponse>.Failure("Invalid Credentials");
        }

        var trader = await _traderRepository.GetByIdAsync(user.Id);
        if (trader != null && trader.IsSuspended)
        {
            return ResultGeneric<TokenResponse>.Failure($"Account is suspended. Reason: {trader.SuspensionReason}");
        }

        return await GenerateAndSaveTokensAsync(user);
    }

    public async Task<ResultGeneric<TokenResponse>> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return ResultGeneric<TokenResponse>.Failure("Invalid access token or refresh token");
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return ResultGeneric<TokenResponse>.Failure("Invalid access token");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ResultGeneric<TokenResponse>.Failure("Invalid access token or refresh token");
        }

        return await GenerateAndSaveTokensAsync(user);
    }

    private async Task<ResultGeneric<TokenResponse>> GenerateAndSaveTokensAsync(Account user)
    {
        string token = await GenerateJwtTokenForUserAsync(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days valid
        await _userManager.UpdateAsync(user);

        return ResultGeneric<TokenResponse>.Success(new TokenResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken
        });
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtConfiguration["JwtSettings:Audience"],
            ValidateIssuer = true,
            ValidIssuer = _jwtConfiguration["JwtSettings:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = RetrieveSigningCredentials().Key,
            ValidateLifetime = false // we want to check expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
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
