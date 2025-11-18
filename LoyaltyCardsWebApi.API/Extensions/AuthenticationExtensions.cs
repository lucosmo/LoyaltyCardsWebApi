using System.Text;
using System.Text.Json;
using LoyaltyCardsWebApi.API.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyCardsWebApi.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, string jwtIssuer, string jwtAudience, string secretKey)
    {
        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            throw new ArgumentException("JWT issuer must not be null, empty, or whitespace.", nameof(jwtIssuer));
        }
        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new ArgumentException("JWT audience must not be null, empty, or whitespace.", nameof(jwtAudience));
        }
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentException("Secret key must not be null, empty, or whitespace.", nameof(secretKey));
        }
            
        services
            .AddAuthentication(options =>
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
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
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
                    await context.Response.WriteAsync(json, Encoding.UTF8, context.HttpContext.RequestAborted);
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
                        : $"Access forbidden.";
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
                    await context.Response.WriteAsync(json, Encoding.UTF8, context.HttpContext.RequestAborted);
                }
            };
        });

        return services;
    }
}