using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Repositories;
using LoyaltyCardsWebApi.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoyalityCardsWebApi.API.Controllers;

[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUser)
    {
        var createdUser = await _userService.CreateUserAsync(newUser);
        return Ok(createdUser);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        if (users == null)
        {
            return NotFound();
        }
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserById(int id)
    {
        var user = await _userService.DeleteAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}