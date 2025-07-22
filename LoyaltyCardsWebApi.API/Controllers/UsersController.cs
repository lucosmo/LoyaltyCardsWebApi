using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCardsWebApi.API.Controllers;

[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICardService _cardService;
    public UsersController(IUserService userService, ICardService cardService)
    {
        _userService = userService;
        _cardService = cardService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUser)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Wrong details");
        }
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (!user.Success)
        {
            return NotFound();
        }
        return Ok(user);
    }

    // GET /api/users/{id}/cards
    [HttpGet("{id}/cards")]
    public async Task<IActionResult> GetCardsByUserId(int id)
    {
        var cards = await _cardService.GetCardsByUserIdAsync(id);
        if (!cards.Success)
        {
            return NotFound();
        }
        return Ok(cards);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserById(int id)
    {
        var user = await _userService.DeleteAsync(id);
        if (!user.Success)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody]UpdatedUserDto updatedUser)
    {
        var isUserUpdated = await _userService.UpdateUserAsync(id, updatedUser);
        if (!isUserUpdated.Success)
        {
            return NotFound();
        }
        return Ok(updatedUser);
    }
}

