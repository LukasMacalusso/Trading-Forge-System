# Details

Date : 2026-05-08 23:49:12

Directory c:\\uct\\Trading forge\\TrandingForgeCode\\Trading-Forge-System\\Trading-Forge-System

Total : 56 files,  2213 codes, 36 comments, 496 blanks, all 2745 lines

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)

## Files
| filename | language | code | comment | blank | total |
| :--- | :--- | ---: | ---: | ---: | ---: |
| [README.md](/README.md) | Markdown | 27 | 0 | 11 | 38 |
| [TraderForge.API/Controllers/IdentityController.cs](/TraderForge.API/Controllers/IdentityController.cs) | C# | 43 | 0 | 12 | 55 |
| [TraderForge.API/Controllers/PricesController.cs](/TraderForge.API/Controllers/PricesController.cs) | C# | 26 | 0 | 5 | 31 |
| [TraderForge.API/Hubs/MarketDataHub.cs](/TraderForge.API/Hubs/MarketDataHub.cs) | C# | 4 | 0 | 2 | 6 |
| [TraderForge.API/Mappers/PricesMapper.cs](/TraderForge.API/Mappers/PricesMapper.cs) | C# | 13 | 0 | 3 | 16 |
| [TraderForge.API/Program.cs](/TraderForge.API/Program.cs) | C# | 78 | 9 | 17 | 104 |
| [TraderForge.API/Properties/launchSettings.json](/TraderForge.API/Properties/launchSettings.json) | JSON | 23 | 0 | 1 | 24 |
| [TraderForge.API/Requests/GetMarketPricesRequest.cs](/TraderForge.API/Requests/GetMarketPricesRequest.cs) | C# | 5 | 0 | 2 | 7 |
| [TraderForge.API/Services/SIgnalRMarketBroadcaster.cs](/TraderForge.API/Services/SIgnalRMarketBroadcaster.cs) | C# | 16 | 0 | 1 | 17 |
| [TraderForge.API/TraderForge.API.csproj](/TraderForge.API/TraderForge.API.csproj) | XML | 20 | 0 | 5 | 25 |
| [TraderForge.API/appsettings.Development.json](/TraderForge.API/appsettings.Development.json) | JSON | 8 | 0 | 1 | 9 |
| [TraderForge.API/appsettings.json](/TraderForge.API/appsettings.json) | JSON | 17 | 0 | 2 | 19 |
| [TraderForge.Application/Common/Result.cs](/TraderForge.Application/Common/Result.cs) | C# | 8 | 0 | 2 | 10 |
| [TraderForge.Application/Common/ResultGeneric.cs](/TraderForge.Application/Common/ResultGeneric.cs) | C# | 9 | 0 | 2 | 11 |
| [TraderForge.Application/DTOs/Commands/RegisterTraderCommand.cs](/TraderForge.Application/DTOs/Commands/RegisterTraderCommand.cs) | C# | 6 | 0 | 1 | 7 |
| [TraderForge.Application/DTOs/Queries/GetMarketPricesQuery.cs](/TraderForge.Application/DTOs/Queries/GetMarketPricesQuery.cs) | C# | 5 | 0 | 1 | 6 |
| [TraderForge.Application/DTOs/Queries/LoginTraderQuery.cs](/TraderForge.Application/DTOs/Queries/LoginTraderQuery.cs) | C# | 6 | 0 | 1 | 7 |
| [TraderForge.Application/Handlers/GetMarketPricesQueryHandler.cs](/TraderForge.Application/Handlers/GetMarketPricesQueryHandler.cs) | C# | 19 | 0 | 7 | 26 |
| [TraderForge.Application/Handlers/LoginTraderQueryHandler.cs](/TraderForge.Application/Handlers/LoginTraderQueryHandler.cs) | C# | 31 | 0 | 8 | 39 |
| [TraderForge.Application/Handlers/RegisterTraderCommandHandler.cs](/TraderForge.Application/Handlers/RegisterTraderCommandHandler.cs) | C# | 47 | 0 | 12 | 59 |
| [TraderForge.Application/TraderForge.Application.csproj](/TraderForge.Application/TraderForge.Application.csproj) | XML | 13 | 0 | 5 | 18 |
| [TraderForge.Domain/Constants/CacheKeys.cs](/TraderForge.Domain/Constants/CacheKeys.cs) | C# | 5 | 0 | 1 | 6 |
| [TraderForge.Domain/Constants/SupportedAssets.cs](/TraderForge.Domain/Constants/SupportedAssets.cs) | C# | 12 | 0 | 1 | 13 |
| [TraderForge.Domain/Entities/Administrator.cs](/TraderForge.Domain/Entities/Administrator.cs) | C# | 14 | 0 | 3 | 17 |
| [TraderForge.Domain/Entities/MarketAsset.cs](/TraderForge.Domain/Entities/MarketAsset.cs) | C# | 9 | 0 | 1 | 10 |
| [TraderForge.Domain/Entities/Trader.cs](/TraderForge.Domain/Entities/Trader.cs) | C# | 14 | 0 | 2 | 16 |
| [TraderForge.Domain/Repositories/IAdministratorRepository.cs](/TraderForge.Domain/Repositories/IAdministratorRepository.cs) | C# | 8 | 0 | 2 | 10 |
| [TraderForge.Domain/Repositories/IMarketAssetRepository.cs](/TraderForge.Domain/Repositories/IMarketAssetRepository.cs) | C# | 7 | 0 | 1 | 8 |
| [TraderForge.Domain/Repositories/ITraderRepository.cs](/TraderForge.Domain/Repositories/ITraderRepository.cs) | C# | 7 | 0 | 2 | 9 |
| [TraderForge.Domain/Services/IIdentityService.cs](/TraderForge.Domain/Services/IIdentityService.cs) | C# | 6 | 0 | 3 | 9 |
| [TraderForge.Domain/Services/IMarketDataBroadcaster.cs](/TraderForge.Domain/Services/IMarketDataBroadcaster.cs) | C# | 6 | 0 | 2 | 8 |
| [TraderForge.Domain/Services/IMarketDataProvider.cs](/TraderForge.Domain/Services/IMarketDataProvider.cs) | C# | 5 | 0 | 1 | 6 |
| [TraderForge.Domain/Services/IMarketService.cs](/TraderForge.Domain/Services/IMarketService.cs) | C# | 5 | 0 | 1 | 6 |
| [TraderForge.Domain/TraderForge.Domain.csproj](/TraderForge.Domain/TraderForge.Domain.csproj) | XML | 12 | 0 | 4 | 16 |
| [TraderForge.Infrastructure/Account.cs](/TraderForge.Infrastructure/Account.cs) | C# | 5 | 0 | 3 | 8 |
| [TraderForge.Infrastructure/Migrations/20260421171958\_AddMarketAssetTable.Designer.cs](/TraderForge.Infrastructure/Migrations/20260421171958_AddMarketAssetTable.Designer.cs) | C# | 41 | 2 | 11 | 54 |
| [TraderForge.Infrastructure/Migrations/20260421171958\_AddMarketAssetTable.cs](/TraderForge.Infrastructure/Migrations/20260421171958_AddMarketAssetTable.cs) | C# | 31 | 3 | 4 | 38 |
| [TraderForge.Infrastructure/Migrations/20260421174331\_InitialIdentitySetup.Designer.cs](/TraderForge.Infrastructure/Migrations/20260421174331_InitialIdentitySetup.Designer.cs) | C# | 206 | 2 | 70 | 278 |
| [TraderForge.Infrastructure/Migrations/20260421174331\_InitialIdentitySetup.cs](/TraderForge.Infrastructure/Migrations/20260421174331_InitialIdentitySetup.cs) | C# | 198 | 3 | 23 | 224 |
| [TraderForge.Infrastructure/Migrations/20260421183124\_AddTraderTable.Designer.cs](/TraderForge.Infrastructure/Migrations/20260421183124_AddTraderTable.Designer.cs) | C# | 223 | 2 | 77 | 302 |
| [TraderForge.Infrastructure/Migrations/20260421183124\_AddTraderTable.cs](/TraderForge.Infrastructure/Migrations/20260421183124_AddTraderTable.cs) | C# | 31 | 3 | 4 | 38 |
| [TraderForge.Infrastructure/Migrations/20260422160223\_AddAdministratorTable.Designer.cs](/TraderForge.Infrastructure/Migrations/20260422160223_AddAdministratorTable.Designer.cs) | C# | 239 | 2 | 83 | 324 |
| [TraderForge.Infrastructure/Migrations/20260422160223\_AddAdministratorTable.cs](/TraderForge.Infrastructure/Migrations/20260422160223_AddAdministratorTable.cs) | C# | 29 | 3 | 4 | 36 |
| [TraderForge.Infrastructure/Migrations/20260423173920\_AddMarketAssetsTable.Designer.cs](/TraderForge.Infrastructure/Migrations/20260423173920_AddMarketAssetsTable.Designer.cs) | C# | 44 | 2 | 12 | 58 |
| [TraderForge.Infrastructure/Migrations/20260423173920\_AddMarketAssetsTable.cs](/TraderForge.Infrastructure/Migrations/20260423173920_AddMarketAssetsTable.cs) | C# | 38 | 3 | 6 | 47 |
| [TraderForge.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs](/TraderForge.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs) | C# | 256 | 1 | 5 | 262 |
| [TraderForge.Infrastructure/Persistence/ApplicationDbContext.cs](/TraderForge.Infrastructure/Persistence/ApplicationDbContext.cs) | C# | 18 | 1 | 2 | 21 |
| [TraderForge.Infrastructure/Repositories/AdministratorRepository.cs](/TraderForge.Infrastructure/Repositories/AdministratorRepository.cs) | C# | 26 | 0 | 6 | 32 |
| [TraderForge.Infrastructure/Repositories/MarketAssetRepository.cs](/TraderForge.Infrastructure/Repositories/MarketAssetRepository.cs) | C# | 19 | 0 | 5 | 24 |
| [TraderForge.Infrastructure/Repositories/TraderRepository.cs](/TraderForge.Infrastructure/Repositories/TraderRepository.cs) | C# | 22 | 0 | 5 | 27 |
| [TraderForge.Infrastructure/Services/BackgroundMarketPollingService.cs](/TraderForge.Infrastructure/Services/BackgroundMarketPollingService.cs) | C# | 71 | 0 | 10 | 81 |
| [TraderForge.Infrastructure/Services/BinanceMarketProvider.cs](/TraderForge.Infrastructure/Services/BinanceMarketProvider.cs) | C# | 20 | 0 | 6 | 26 |
| [TraderForge.Infrastructure/Services/CachedMarketService.cs](/TraderForge.Infrastructure/Services/CachedMarketService.cs) | C# | 17 | 0 | 5 | 22 |
| [TraderForge.Infrastructure/Services/IdentityService.cs](/TraderForge.Infrastructure/Services/IdentityService.cs) | C# | 107 | 0 | 24 | 131 |
| [TraderForge.Infrastructure/TraderForge.Infrastructure.csproj](/TraderForge.Infrastructure/TraderForge.Infrastructure.csproj) | XML | 23 | 0 | 4 | 27 |
| [docker-compose.yml](/docker-compose.yml) | YAML | 15 | 0 | 2 | 17 |

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)