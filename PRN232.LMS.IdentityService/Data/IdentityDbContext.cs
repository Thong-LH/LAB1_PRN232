using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PRN232.LMS.IdentityService.Entities;

namespace PRN232.LMS.IdentityService.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(user => user.UserId);

            entity.Property(user => user.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(user => user.Username)
                .IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshToken");
            entity.HasKey(refreshToken => refreshToken.RefreshTokenId);

            entity.Property(refreshToken => refreshToken.Token)
                .HasMaxLength(200)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(refreshToken => refreshToken.ReplacedByToken)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(refreshToken => refreshToken.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(refreshToken => refreshToken.Token)
                .IsUnique();
        });

        // Seed Admin User
        var admin = new User
        {
            UserId = 1,
            Username = "admin",
            Role = "Admin"
        };
        var passwordHasher = new PasswordHasher<User>();
        admin.PasswordHash = passwordHasher.HashPassword(admin, "123456");

        modelBuilder.Entity<User>().HasData(admin);
    }
}
