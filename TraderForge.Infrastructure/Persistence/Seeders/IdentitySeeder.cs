using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;
using TraderForge.Infrastructure;

namespace TraderForge.Infrastructure.Persistence.Seeders;

public static class IdentitySeeder
{
    private class AdminConfig
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public static async Task SeedDefaultAdminsAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");
        var adminConfigs = GetAdminConfigurations(configuration);

        if (!adminConfigs.Any())
        {
            logger.LogWarning("No default administrators found in configuration. Skipping seeding.");
            return;
        }

        await ProcessAdminConfigurationsAsync(serviceProvider, adminConfigs, logger);
    }

    private static List<AdminConfig> GetAdminConfigurations(IConfiguration configuration)
    {
        return configuration.GetSection("AdminSettings:DefaultAdmins").Get<List<AdminConfig>>() ?? new List<AdminConfig>();
    }

    private static async Task ProcessAdminConfigurationsAsync(IServiceProvider serviceProvider, List<AdminConfig> configs, ILogger logger)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<Account>>();
        var adminRepository = serviceProvider.GetRequiredService<IAdministratorRepository>();

        foreach (var config in configs)
        {
            if (string.IsNullOrWhiteSpace(config.Password))
            {
                logger.LogWarning($"Password missing for {config.Email}. Skipping.");
                continue;
            }

            await ProcessSingleAdminAsync(userManager, adminRepository, config, logger);
        }
    }

    private static async Task ProcessSingleAdminAsync(UserManager<Account> userManager, IAdministratorRepository adminRepository, AdminConfig config, ILogger logger)
    {
        var existingIdentityAdmin = await userManager.FindByEmailAsync(config.Email);

        if (existingIdentityAdmin == null)
        {
            await CreateNewAdminAsync(userManager, adminRepository, config, logger);
        }
        else
        {
            await UpdateExistingAdminAsync(userManager, adminRepository, existingIdentityAdmin, config, logger);
        }
    }

    private static async Task CreateNewAdminAsync(UserManager<Account> userManager, IAdministratorRepository adminRepository, AdminConfig config, ILogger logger)
    {
        var adminUser = new Account
        {
            Id = Guid.NewGuid().ToString(),
            UserName = config.Email,
            Email = config.Email
        };

        var result = await userManager.CreateAsync(adminUser, config.Password);

        if (result.Succeeded)
        {
            await AssignSystemAdminRoleAsync(userManager, adminUser);
            logger.LogInformation($"Successfully seeded Identity account for admin: {config.Email}");

            await EnsureDomainAdminExistsAsync(adminRepository, adminUser.Id, adminUser.Email, logger);
        }
        else
        {
            logger.LogError($"Failed to seed admin {config.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    private static async Task UpdateExistingAdminAsync(UserManager<Account> userManager, IAdministratorRepository adminRepository, Account existingAdmin, AdminConfig config, ILogger logger)
    {
        await EnsurePasswordIsSynchronizedAsync(userManager, existingAdmin, config.Password, logger);
        await EnsureDomainAdminExistsAsync(adminRepository, existingAdmin.Id, existingAdmin.Email, logger);
    }

    private static async Task AssignSystemAdminRoleAsync(UserManager<Account> userManager, Account user)
    {
        await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "SystemAdmin"));
    }

    private static async Task EnsurePasswordIsSynchronizedAsync(UserManager<Account> userManager, Account user, string expectedPassword, ILogger logger)
    {
        var isPasswordValid = await userManager.CheckPasswordAsync(user, expectedPassword);
        
        if (!isPasswordValid)
        {
            logger.LogInformation($"Updating password for existing admin: {user.Email}");
            
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, expectedPassword);
            
            if (!resetResult.Succeeded)
            {
                logger.LogError($"Failed to reset password for {user.Email}: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task EnsureDomainAdminExistsAsync(IAdministratorRepository adminRepository, string id, string email, ILogger logger)
    {
        var existingDomainAdmin = await adminRepository.GetByIdAsync(id);
        
        if (existingDomainAdmin == null)
        {
            var domainAdmin = new Administrator(id, email);
            await adminRepository.AddAsync(domainAdmin);
            logger.LogInformation($"Successfully ensured domain Administrator record exists for: {email}");
        }
    }
}
