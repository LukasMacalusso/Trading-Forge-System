using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TraderForge.API.Hubs;
using TraderForge.API.Services;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Factories;
using TraderForge.Domain.Interfaces;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Infrastructure;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Infrastructure.Repositories;
using TraderForge.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// -- Load adminCredentials.env for local development (Docker loads it via env_file) -- //
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "adminCredentials.env");
if (!File.Exists(envPath))
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "adminCredentials.env");
if (File.Exists(envPath))
{
    var envConfig = new Dictionary<string, string?>();
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            continue;
        var idx = trimmed.IndexOf('=', StringComparison.Ordinal);
        if (idx > 0)
        {
            var key = trimmed[..idx].Replace("__", ":");
            var value = trimmed[(idx + 1)..];
            envConfig[key] = value;
        }
    }
    builder.Configuration.AddInMemoryCollection(envConfig);
}

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
builder.Services.AddScoped<ITraderFactory, TraderFactory>();
builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
builder.Services.AddScoped<IMarketAssetRepository, MarketAssetRepository>();
builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<IStrategyRepository, StrategyRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<ISubscriptionLimitGuard, SubscriptionLimitGuard>();
builder.Services.AddTransient<RegisterTraderCommandHandler>();
builder.Services.AddTransient<LoginTraderQueryHandler>();
builder.Services.AddTransient<ChangeSubscriptionCommandHandler>();
builder.Services.AddTransient<GetAllPlansQueryHandler>();
builder.Services.AddTransient<GetTraderPlanQueryHandler>();
builder.Services.AddTransient<CreatePlanCommandHandler>();
builder.Services.AddTransient<UpdatePlanCommandHandler>();
builder.Services.AddTransient<DeletePlanCommandHandler>();
builder.Services.AddTransient<CreateStrategyCommandHandler>();
builder.Services.AddTransient<UpdateStrategyStateCommandHandler>();
builder.Services.AddTransient<AddPositionCommandHandler>();
builder.Services.AddTransient<RemovePositionCommandHandler>();
builder.Services.AddTransient<GetActivePortfolioQueryHandler>();
builder.Services.AddTransient<GetStrategiesQueryHandler>();
builder.Services.AddTransient<GetPositionsQueryHandler>();
builder.Services.AddHostedService<TrialExpirationService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// -- CQRS Handlers -- //
builder.Services.AddTransient<GetMarketPricesQueryHandler>();

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
        policyBuilder.WithOrigins("http://localhost:3000")
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
