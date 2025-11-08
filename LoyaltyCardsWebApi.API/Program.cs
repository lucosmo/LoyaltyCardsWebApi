using Microsoft.EntityFrameworkCore;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LoyaltyCardsWebApi.API.Middleware;
using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.ExceptionHandling;
using LoyaltyCardsWebApi.API.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using LoyaltyCardsWebApi.API.Models;


DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSettings = builder.Configuration.GetSection("JWTSettings");
var secretKey = jwtSettings["JWT_Secret"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("Key for JWT authentication is not configured or is empty");
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => 
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["JWT_Issuer"],
        ValidAudience = jwtSettings["JWT_Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            
            if (context.Response.HasStarted)
            {
                return;
            }
                            
            var statusCode = StatusCodes.Status401Unauthorized;
            var details = !string.IsNullOrEmpty(context.Error)
                ? context.Error : !string.IsNullOrEmpty(context.ErrorDescription)
                    ? context.ErrorDescription : "Authentication failed.";
            var title = "Unauthorized.";

            var problemDetails = ProblemDetailsHelper.CreateProblemDetails(
                    context.HttpContext,
                    title,
                    statusCode,
                    details
                );
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json; charset=utf-8";
            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json, Encoding.UTF8);
        },
        OnForbidden = async context =>
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var statusCode = StatusCodes.Status403Forbidden;
            var user = context.HttpContext.User;
            var path = context.HttpContext.Request.Path;
            var method = context.HttpContext.Request.Method;
            var details = user.Identity?.IsAuthenticated == true
                ? $"Access to {method} {path} is forbidden."
                : $"Access forbidden";
            var title = "Access forbidden.";

            var problemDetails = ProblemDetailsHelper.CreateProblemDetails(
                    context.HttpContext,
                    title,
                    statusCode,
                    details
                );
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json; charset=utf-8";
            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json, Encoding.UTF8);
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = (context) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenRevocationMiddleware>();
app.MapControllers();
app.Run();

