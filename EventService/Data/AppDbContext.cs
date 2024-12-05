using Microsoft.EntityFrameworkCore;
using EventService.Models;

namespace EventService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<UserEvent> UserEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEvent>()
            .Property(e => e.CreatedOn)
            .HasDefaultValueSql("NOW()");
    }
}