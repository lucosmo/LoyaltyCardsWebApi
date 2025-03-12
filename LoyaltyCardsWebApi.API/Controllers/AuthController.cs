using System.IdentityModel.Tokens.Jwt;
using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        try
        {
            var token = await _authService.LoginAsync(loginDto);
            if (token.Success == false)
            {
                return BadRequest(token.Error);
            }
            
            return Ok(new {Token = token});
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An unexpected error occurred during login.");
        }  
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(500, "An unexpected error occurred during logout.");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto newUserDto)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest("Invalid registration data.");
            }

            var result = await _authService.RegisterAsync(newUserDto);
            if (result.Success == false)
            {
                return BadRequest(result.Error);
            }

            return Ok(new {Message = "User registered successfully", User = result.Value});
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An unexpected error occurred during registration.");
        }
        
    }
}