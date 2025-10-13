using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
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
        if (!result.Success)
        {
            return BadRequest(result.Value);
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        if (!users.Success)
        {
            return NotFound();
        }
        return Ok(users);
    }

    [Authorize]
    [HttpGet("myaccount")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.Success)
        {
            return Unauthorized();
        }
        return Ok(user);
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
        if (!user.Success)
        {
            return NotFound();
        }
        return Ok(user);
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
        if (!cards.Success)
        {
            return NotFound();
        }
        return Ok(cards);
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
        if (!user.Success)
        {
            return NotFound();
        }
        return Ok(user);
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
        if (!isUserUpdated.Success)
        {
            return NotFound();
        }
        return Ok(updatedUser);
    }
}

