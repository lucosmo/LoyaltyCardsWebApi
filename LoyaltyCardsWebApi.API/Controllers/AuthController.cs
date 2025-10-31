using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Controllers.Results;
using LoyaltyCardsWebApi.API.Data;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCardsWebApi.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRequestContext _requestContext;
    private const string InvalidCredentialsMessage = "Invalid credentials";

    public AuthController(IAuthService authService, IRequestContext requestContext)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var tokenResult = await _authService.LoginAsync(loginDto);
        return new ApiResult<string>(tokenResult);  
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var tokenResult = _authService.GetTokenAuthHeader();
        if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
        {
            return new ApiResult<string>(tokenResult);
        }

        var userIdResult = _authService.GetUserId();
        if (!userIdResult.Success)
        {
            return new ApiResult<int>(userIdResult);
        }

        var revokeResult = await _authService.AddRevokedTokenAsync(tokenResult.Value, userIdResult.Value);
        if (!revokeResult.Success)
        {
            return new ApiResult<string>(revokeResult);
        }
        
        return new ApiResult<object?>(Result<object?>.NoContent());
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto newUserDto)
    {
        var result = await _authService.RegisterAsync(newUserDto);
        var location = Url.Action(nameof(UsersController.GetUserById), "Users", new { id = result.Value?.Id });
        if(result.Success && result.Value is not null && location is not null)
        {
            return new ApiResult<UserDto>(Result<UserDto>.Created(result.Value, location));    
        }
        return new ApiResult<UserDto>(result);
    }
}