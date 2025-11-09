using Microsoft.EntityFrameworkCore;
using LoyaltyCardsWebApi.API.Models;
using LoyaltyCardsWebApi.API.Common;

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

        modelBuilder.Entity<User>(c =>
        {
            c.Property(u => u.PasswordHash).IsRequired();
            c.Property(u => u.Role)
             .HasDefaultValue(UserRole.User);
        });
    }
}