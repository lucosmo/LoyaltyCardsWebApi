using LoyaltyCardsWebApi.API.Common;
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

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUser, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateUserAsync(newUser, cancellationToken);
        return new ApiResult<UserDto>(result);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return new ApiResult<List<UserDto>>(users);
    }

    [Authorize]
    [HttpGet("myaccount")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var user = await _userService.GetCurrentUserAsync(currentUserId, cancellationToken);
        return new ApiResult<UserDto>(user);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var user = await _userService.GetUserByIdAsync(id, currentUserId, cancellationToken);
        return new ApiResult<UserDto>(user);
    }

    // GET /api/users/{id}/cards
    [Authorize]
    [HttpGet("{id}/cards")]
    public async Task<IActionResult> GetCardsByUserId(int id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var cards = await _cardService.GetCardsByUserIdAsync(id, currentUserId, cancellationToken);
        return new ApiResult<IEnumerable<CardDto>>(cards);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserById(int id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var user = await _userService.DeleteAsync(id, currentUserId, cancellationToken);
        return new ApiResult<UserDto>(user);
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody]UpdatedUserDto updatedUser, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var isUserUpdated = await _userService.UpdateUserAsync(id, updatedUser, currentUserId, cancellationToken);
        return new ApiResult<bool>(isUserUpdated);
    }
}

