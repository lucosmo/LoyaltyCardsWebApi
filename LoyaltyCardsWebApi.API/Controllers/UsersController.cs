using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LoyalityCardsWebApi.API.Controllers;

[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserDto newUser)
    {
        var newUserModel = new User
        {
            UserName = newUser.UserName,
            Email = newUser.Email,
            Password = newUser.Password,
            AccountCreatedDate = DateTime.UtcNow
            
        };

        var createdUser = await _userRepository.CreateAsync(newUserModel);

        return Ok(createdUser);

    }
}