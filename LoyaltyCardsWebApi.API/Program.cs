using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data;
using LoyaltyCardsWebApi.API.ExceptionHandling;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Middleware;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;


DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSettings = builder.Configuration.GetSection("JWTSettings");
var secretKey = jwtSettings["JWT_Secret"];
var jwtIssuer = jwtSettings["JWT_Issuer"];
var jwtAudience = jwtSettings["JWT_Audience"];

if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException("JWT Secret is not configured");
}
if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException("JWT Issuer is not configured");
}
if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("JWT Audience is not configured");
}

builder.Host.UseSerilog((ctx, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddJwtAuthentication(jwtIssuer, jwtAudience, secretKey);
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestContextMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenRevocationMiddleware>();
app.UseMiddleware<UserContextMiddleware>();
app.MapControllers();
app.Run();

