using System.Text;
using Microsoft.EntityFrameworkCore;
using TraderForge.API.Hubs;
using TraderForge.API.Services;
using TraderForge.Application.Handlers;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Domain.Services;
using TraderForge.Domain.Repositories;
using TraderForge.Infrastructure.Repositories;
using TraderForge.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddTransient<GetMarketPricesQueryHandler>();

builder.Services.AddHttpClient<IMarketDataProvider, BinanceMarketProvider>();
builder.Services.AddSingleton<IMarketService, CachedMarketService>();
builder.Services.AddHostedService<BackgroundMarketPollingService>();
builder.Services.AddSingleton<IMarketDataBroadcaster, SignalRMarketDataBroadcaster>();
builder.Services.AddSignalR();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMarketAssetRepository, MarketAssetRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.SetIsOriginAllowed(_ => true) // Adjust this for production security
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // REQUIRED FOR SIGNALR
    });
});

var app = builder.Build();

using Microsoft.IdentityModel.Tokens;
using TraderForge.Application.Handlers;
using TraderForge.Domain.Interfaces;
using TraderForge.Infrastructure;
using TraderForge.Infrastructure.Persistence;
using TraderForge.Infrastructure.Repositories;
using TraderForge.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// -- Configure ASP.NET Core Identity -- //
builder.Services.AddIdentityCore<Account>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<ApplicationDbContext>();


// - Register Services for the DEPENDENCY INJECTION - //
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ITraderRepository, TraderRepository>();
builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
builder.Services.AddTransient<RegisterTraderCommandHandler>(); 
builder.Services.AddTransient<LoginTraderQueryHandler>(); 
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// - Register JWT Authentication - //
builder.Services.AddAuthentication(options =>
    {
        // This tells ASP.NET to default to looking for a JWT token
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


// -- Register database context -- //
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// -- Initialize app -- //
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<MarketDataHub>("/hubs/market");

app.Run();

// - Auth - //
app.UseAuthentication(); 
app.UseAuthorization();

// - Controller - //
app.MapControllers();

app.Run();
