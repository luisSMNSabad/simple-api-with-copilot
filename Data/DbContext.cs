using Microsoft.EntityFrameworkCore;
using SecureApp.Models;

namespace SecureApp.Data;

public class SecureAppDbContext : DbContext
{
    public SecureAppDbContext(DbContextOptions<SecureAppDbContext> options)
    : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}