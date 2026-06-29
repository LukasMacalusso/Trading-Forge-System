using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TraderForge.API.Hubs;
using TraderForge.API.Services;
using TraderForge.Application.Handlers;

using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Infrastructure;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Infrastructure.Repositories;
using TraderForge.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// -- Core Services Registration -- //
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

// -- Database Context -- //
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -- ASP.NET Core Identity -- //
builder.Services.AddIdentityCore<Account>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<ApplicationDbContext>();

// -- Authentication & JWT -- //
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    };
});

// -- Dependency Injection (Repositories & Services) -- //
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ITraderRepository, TraderRepository>();

builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();

builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<IStrategyRepository, StrategyRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ISubscriptionLimitGuard, SubscriptionLimitGuard>();
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// -- Bot Engine (Task 4.1) -- //
builder.Services.AddScoped<IBotNodeRepository, BotNodeRepository>();
builder.Services.AddScoped<IBotEdgeRepository, BotEdgeRepository>();
builder.Services.AddScoped<IStrategyExecutionRepository, StrategyExecutionRepository>();

builder.Services.AddSingleton<IMarketDataEventBus, MarketDataEventBus>();
builder.Services.AddSingleton<IStrategyEngine>(sp => sp.GetRequiredService<StrategyEngineService>());
builder.Services.AddHostedService<StrategyEngineService>();

builder.Services.AddTransient<AddBotNodeCommandHandler>();
builder.Services.AddTransient<UpdateBotNodeCommandHandler>();
builder.Services.AddTransient<RemoveBotNodeCommandHandler>();
builder.Services.AddTransient<AddBotEdgeCommandHandler>();
builder.Services.AddTransient<RemoveBotEdgeCommandHandler>();
builder.Services.AddTransient<StartEngineCommandHandler>();
builder.Services.AddTransient<StopEngineCommandHandler>();

builder.Services.AddTransient<RegisterTraderCommandHandler>();
builder.Services.AddTransient<LoginTraderQueryHandler>();
builder.Services.AddTransient<ChangeSubscriptionCommandHandler>();
builder.Services.AddTransient<CancelSubscriptionCommandHandler>();
builder.Services.AddTransient<GetAllPlansQueryHandler>();
builder.Services.AddTransient<GetTraderPlanQueryHandler>();
builder.Services.AddTransient<CreatePlanCommandHandler>();
builder.Services.AddTransient<UpdatePlanCommandHandler>();
builder.Services.AddTransient<DeletePlanCommandHandler>();
builder.Services.AddTransient<CreateStrategyCommandHandler>();
builder.Services.AddTransient<UpdateStrategyStateCommandHandler>();
builder.Services.AddTransient<BuyPositionCommandHandler>();
builder.Services.AddTransient<SellPositionCommandHandler>();
builder.Services.AddTransient<GetActivePortfolioQueryHandler>();
builder.Services.AddTransient<GetStrategiesQueryHandler>();
builder.Services.AddTransient<GetPositionsQueryHandler>();
builder.Services.AddTransient<GetTransactionsQueryHandler>();
builder.Services.AddTransient<ResetSimulationCommandHandler>();
builder.Services.AddTransient<GetOrdersQueryHandler>();
builder.Services.AddTransient<GetAllTradersQueryHandler>();
builder.Services.AddTransient<SuspendTraderCommandHandler>();
builder.Services.AddScoped<UnsuspendTraderCommandHandler>();

builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// -- CQRS Handlers -- //
builder.Services.AddTransient<GetMarketPricesQueryHandler>();
builder.Services.AddTransient<GetHistoricalCandlesQueryHandler>();

// -- Market Data Services -- //
builder.Services.AddHttpClient<IMarketDataProvider, BinanceMarketProvider>();
builder.Services.AddSingleton<IMarketService, CachedMarketService>();
builder.Services.AddHostedService<BackgroundMarketPollingService>();
builder.Services.AddSingleton<IMarketDataBroadcaster, SignalRMarketDataBroadcaster>();

// -- CORS -- //
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// -- Initialize app -- //
var app = builder.Build();

// --- Identity Seeder Execution --- //
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Applying pending database migrations...");

        await context.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully.");

        logger.LogInformation("Attempting to seed default Identity administrators...");
        await TraderForge.Infrastructure.Persistence.Seeders.IdentitySeeder.SeedDefaultAdminsAsync(services, configuration);
        logger.LogInformation("Identity seeding completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "A fatal error occurred during database migration or seeding.");
    }
}

// -- Middleware Pipeline -- //
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MarketDataHub>("/hubs/market");

app.Run();
