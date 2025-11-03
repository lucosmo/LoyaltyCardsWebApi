using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LoyaltyCardsWebApi.API.Common;
using LoyaltyCardsWebApi.API.Data;
using LoyaltyCardsWebApi.API.Data.DTOs;
using LoyaltyCardsWebApi.API.Extensions;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;

namespace LoyaltyCardsWebApi.API.Services;

public class AuthService : IAuthService
{
    private readonly IRequestContext _requestContext;
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IRequestContext requestContext,
        IAuthRepository authRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IJwtService jwtService)
    {
        _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
    }
    public async Task<Result<string>> LoginAsync(LoginDto loginDto)
    {
        if (loginDto is null)
        {
            return Result<string>.BadRequest("Login data is required.");
        }
        if (string.IsNullOrEmpty(loginDto.Email))
        {
            return Result<string>.BadRequest("Email is required.");
        }
        if (string.IsNullOrEmpty(loginDto.Password))
        {
            return Result<string>.BadRequest("Password is required.");
        }
        var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
        
        if (user==null || user.Password != loginDto.Password)
        {
            return Result<string>.Unauthorized("Invalid credentials.");
        }
        
        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email);
        if (string.IsNullOrEmpty(token))
        {
            return Result<string>.Fail("Token generation failed");
        }
        return Result<string>.Ok(token);
    }

    public async Task<Result<UserDto>> RegisterAsync(CreateUserDto newUserDto)
    {
        if (newUserDto is null)
        {
            return Result<UserDto>.BadRequest("Registration data is required.");
        }
        if (string.IsNullOrWhiteSpace(newUserDto.Email))
        {
            return Result<UserDto>.BadRequest("Email is required.");
        }
        if (string.IsNullOrWhiteSpace(newUserDto.Password))
        {
            return Result<UserDto>.BadRequest("Password is required.");
        }
        if (string.IsNullOrWhiteSpace(newUserDto.UserName))
        {
            return Result<UserDto>.BadRequest("Username is required.");
        }

        var existingUser = await _userRepository.GetUserByEmailAsync(newUserDto.Email);
        if (existingUser != null)
        {
            return Result<UserDto>.Conflict($"User with this email: {newUserDto.Email} already exists");
        }

        var newUserModel = new User
        {
            UserName = newUserDto.UserName,
            Email = newUserDto.Email,
            Password = newUserDto.Password,
            AccountCreatedDate = DateTime.UtcNow  
        };
        var createdUser = await _userRepository.CreateAsync(newUserModel);
        if (createdUser is null)
        {
            return Result<UserDto>.Fail($"Registration failed for this email: {newUserDto.Email}");
        }

        var userDto = createdUser.ToDto();
        return Result<UserDto>.Ok(userDto);
    }

    public Result<int> GetUserId()
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Result<int>.Unauthorized("User ID not found in token");
        }
        return Result<int>.Ok(userId.Value);
    }
    public Result<string> GetTokenAuthHeader()
    {
        var authHeader = _requestContext.Authorization;
        if (authHeader is null || authHeader.StartsWith("Bearer ") == false)
        {
            return Result<string>.Unauthorized("Token not found in Authorization header");
        }
        var token = authHeader.Substring("Bearer ".Length).Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Result<string>.BadRequest("Token is empty.");
        }
        return Result<string>.Ok(token);
    }

    public DateTime? GetTokenExpiryDate()
    {
        var expiryDateClaim = _requestContext.ExpiryTime;
        if (string.IsNullOrEmpty(expiryDateClaim) || !long.TryParse(expiryDateClaim, out var expiryDateSeconds))
        {
            return null;
        }
        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expiryDateSeconds).UtcDateTime;
        return expiryDate;
    }

    public async Task<Result<string>> AddRevokedTokenAsync(string token, int userId)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result<string>.BadRequest("Token is required.");
        }
        if (userId <= 0)
        {
            return Result<string>.BadRequest("Invalid user ID.");
        }
        if (userId != _currentUserService.UserId)
        {
            return Result<string>.Forbidden("No permission.");
        }
        var tokenExpiryDateTime = GetTokenExpiryDate();
        if (tokenExpiryDateTime is null)
        {
            return Result<string>.NotFound("Token expiry date not found");
        }

        if (tokenExpiryDateTime < DateTime.UtcNow)
        {
            return Result<string>.Unauthorized("Token expiry date not found");
        }

        var revokedToken = await _authRepository.AddRevokedTokenAsync(token, tokenExpiryDateTime.Value, userId);
        if (revokedToken is null)
        {
            return Result<string>.Fail("Failed to revoke token.");
        }
              
        return Result<string>.Ok("Token successfully revoked");
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var isRevokedResult = await _authRepository.IsTokenRevokedAsync(token);
        return isRevokedResult;
    }

    public async Task<Result<bool>> RevokeAllTokensForUserAsync(int userId)
    {
        if (userId <= 0)
        {
            return Result<bool>.BadRequest("Invalid user ID.");
        }

        if (userId != _currentUserService.UserId)
        {
            return Result<bool>.Forbidden("No permission.");
        }

        await _authRepository.RevokeAllTokensForUserAsync(userId);
        return Result<bool>.Ok(true);
    }

}