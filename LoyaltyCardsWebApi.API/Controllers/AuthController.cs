using System.IdentityModel.Tokens.Jwt;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LoyaltyCardsWebApi.API.Controllers;

[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string InvalidCredentialsMessage = "Invalid credentials";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(InvalidCredentialsMessage);
        }

        var tokenResult = await _authService.LoginAsync(loginDto);
        if (!tokenResult.Success)
        {
            return BadRequest(tokenResult.Error);
        }
        
        return Ok(new {Token = tokenResult});  
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var tokenResult = _authService.GetTokenAuthHeader();
        if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
        {
            return BadRequest(tokenResult.Error);
        }

        var userIdResult = _authService.GetUserId();
        if (!userIdResult.Success)
        {
            return BadRequest(userIdResult.Error);
        }

        var expiryDateResult = _authService.GetTokenExpiryDate();
        if (!expiryDateResult.Success)
        {
            return BadRequest(expiryDateResult.Error);
        }
    
        var revokeResult = await _authService.AddRevokedTokenAsync(tokenResult.Value, expiryDateResult.Value, userIdResult.Value);
        if (!revokeResult.Success)
        {
            return BadRequest(revokeResult.Error);
        }

        return Ok(new {Message = "Successfully logged out"});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto newUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid registration data.");
        }

        var result = await _authService.RegisterAsync(newUserDto);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            actionName: "GetUserById",
            controllerName: "Users",
            routeValues: new { id = result.Value?.Id },
            value: result.Value
        );              
    }
}