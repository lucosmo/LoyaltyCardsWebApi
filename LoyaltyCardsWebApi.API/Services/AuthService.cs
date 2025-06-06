using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LoyalityCardsWebApi.API.Data.DTOs;
using LoyalityCardsWebApi.API.Models;
using LoyalityCardsWebApi.API.Repositories;

namespace LoyaltyCardsWebApi.API.Services;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IHttpContextAccessor httpContextAccessor, IAuthRepository authRepository, IUserRepository userRepository, IJwtService jwtService)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
    }
    public async Task<Result<string>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
        
        if (user==null || user.Password != loginDto.Password)
        {
            return Result<string>.Fail("Invalid credentials");
        }
        
        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email);
        if (string.IsNullOrEmpty(token))
        {
            return Result<string>.Fail("Token generation failed");
        }
        return Result<string>.Ok(token);
    }

    public async Task<Result<string>> RegisterAsync(CreateUserDto newUserDto)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(newUserDto.Email);
        if (existingUser != null)
        {
            return Result<string>.Fail($"User with this email: {newUserDto.Email} already exists");
        }

        var newUserModel = new User
        {
            UserName = newUserDto.UserName,
            Email = newUserDto.Email,
            Password = newUserDto.Password,
            AccountCreatedDate = DateTime.UtcNow  
        };
        var createdUser = await _userRepository.CreateAsync(newUserModel);
        if (createdUser == null)
        {
            return Result<string>.Fail($"Registration failed for this email: {newUserDto.Email}");
        }
        return Result<string>.Ok($"User {createdUser.UserName} registered successfully");
    }

    public Result<string> GetTokenAuthHeader()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (authHeader == null || authHeader.StartsWith("Bearer ") == false )
        {
            return Result<string>.Fail("Token not found in Authorization header");
        }
        var token = authHeader.Substring("Bearer ".Length).Trim();

        return Result<string>.Ok(token);
    }

    public Result<int> GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Result<int>.Fail("User ID not found in token");
        }
        return Result<int>.Ok(userId);
    }

    public Result<DateTime> GetTokenExpiryDate()
    {
        var expiryDateClaim = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (string.IsNullOrEmpty(expiryDateClaim) || !long.TryParse(expiryDateClaim, out var expiryDateSeconds))
        {
            return Result<DateTime>.Fail("Token expiry date not found");
        }
        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expiryDateSeconds).UtcDateTime;
        return Result<DateTime>.Ok(expiryDate);
    }

    public async Task<Result<string>> AddRevokedTokenAsync(string token, DateTime expiryDate, int userId)
    {
        var revokedToken = await _authRepository.AddRevokedTokenAsync(token, expiryDate, userId);
        if (revokedToken == null)
        {
            return Result<string>.Fail("Failed to revoke token. Repository returned null.");
        }
        
        return Result<string>.Ok(revokedToken.Token);        
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        var isRevokedResult = await _authRepository.IsTokenRevokedAsync(token);
        return isRevokedResult;
    }

    public async Task<Result<bool>> RevokeAllTokensForUserAsync(int userId)
    {
        await _authRepository.RevokeAllTokensForUserAsync(userId);
        return Result<bool>.Ok(true);
    }

}