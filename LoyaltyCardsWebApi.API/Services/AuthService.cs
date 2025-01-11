using LoyalityCardsWebApi.API.Data.DTOs;

namespace LoyaltyCardsWebApi.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public AuthService(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }
    public async Task<string> LoginAsync(LoginDto loginDto)
    {
        var isValidCredential = await _userService.ValidateCredentialsAsync(loginDto);
        if (isValidCredential == false)
        {
            return String.Empty;
        }
        
        var user = await _userService.GetUserByEmailAsync(loginDto);
        if (user == null)
        {
            return String.Empty;
        }
        
        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email);
        return token;
    }
}