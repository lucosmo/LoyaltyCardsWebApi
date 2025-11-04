using LoyaltyCardsWebApi.API.Controllers.Results;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCardsWebApi.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICardService _cardService;
    private readonly ICurrentUserService _currentUserService;
    public UsersController(IUserService userService, ICardService cardService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _cardService = cardService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUser)
    {
        var result = await _userService.CreateUserAsync(newUser);
        return new ApiResult<UserDto>(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return new ApiResult<List<UserDto>>(users);
    }

    [Authorize]
    [HttpGet("myaccount")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var currentUserId = _currentUserService.UserId;
        var user = await _userService.GetUserByIdAsync(currentUserId);
        return new ApiResult<UserDto>(user);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
        {
            return Unauthorized("No permission to perform action.");
        }
        var user = await _userService.GetUserByIdAsync(id, currentUserId.Value);
        return new ApiResult<UserDto>(user);
    }

    // GET /api/users/{id}/cards
    [Authorize]
    [HttpGet("{id}/cards")]
    public async Task<IActionResult> GetCardsByUserId(int id)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
        {
            return Unauthorized("No permission to perform action.");
        }
        var cards = await _cardService.GetCardsByUserIdAsync(id, currentUserId);
        return new ApiResult<IEnumerable<CardDto>>(cards);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserById(int id)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
        {
            return Unauthorized("No permission to perform action.");
        }
        var user = await _userService.DeleteAsync(id, currentUserId.Value);
        return new ApiResult<UserDto>(user);
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody]UpdatedUserDto updatedUser)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
        {
            return Unauthorized("No permission to perform action.");
        }
        var isUserUpdated = await _userService.UpdateUserAsync(id, updatedUser, currentUserId.Value);
        return new ApiResult<bool>(isUserUpdated);
    }
}

