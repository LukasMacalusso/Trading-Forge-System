using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TraderForge.API.Hubs;
using TraderForge.API.Services;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Interfaces;
using TraderForge.Domain.Repositories;
using TraderForge.Domain.Services;
using TraderForge.Infrastructure;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Infrastructure.Repositories;
using TraderForge.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// -- Core Services Registration -- //
builder.Services.AddOpenApi();
builder.Services.AddControllers();
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
builder.Services.AddScoped<IMarketAssetRepository, MarketAssetRepository>();

// -- CQRS Handlers -- //
builder.Services.AddTransient<RegisterTraderCommandHandler>();
builder.Services.AddTransient<LoginTraderQueryHandler>();
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
        policyBuilder.WithOrigins("http://localhost:3000") // Replace with actual frontend URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); 
    });
});


var app = builder.Build();

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
