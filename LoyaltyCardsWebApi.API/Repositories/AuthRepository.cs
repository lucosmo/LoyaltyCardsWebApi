using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Repositories;
using Microsoft.EntityFrameworkCore;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _appDbContext;

    public AuthRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    
    public async Task<RevokedToken> AddRevokedTokenAsync(string token, DateTime expiryDate, int userId, CancellationToken cancellationToken = default)
    {
        var revokedToken = new RevokedToken
        {
            Token = token,
            ExpiryTime = expiryDate,
            UserId = userId
        };

        var createdRevokedToken = await _appDbContext.RevokedToken.AddAsync(revokedToken, cancellationToken) 
                                ?? throw new InvalidOperationException("Failed to insert revoked token into the database.");
        
        await _appDbContext.SaveChangesAsync(cancellationToken);
        
        return createdRevokedToken.Entity;
    }

    public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.RevokedToken.AnyAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task RevokeAllTokensForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _appDbContext.RevokedToken
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);
        
        _appDbContext.RevokedToken.RemoveRange(tokens);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}