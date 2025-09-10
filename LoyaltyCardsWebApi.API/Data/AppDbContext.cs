using Microsoft.EntityFrameworkCore;
using LoyaltyCardsWebApi.API.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RevokedToken> RevokedToken => Set<RevokedToken>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Card>(c =>
        {
            c.HasIndex(card => new { card.Barcode, card.UserId })
             .IsUnique();
        });
    }
}