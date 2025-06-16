using Microsoft.EntityFrameworkCore;
using Testes.Models;

namespace Testes.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Donation> Donations { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       /* modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.DocumentNumber).IsUnique();*/
       
       modelBuilder.Entity<Donation>()
           .HasOne(d => d.User)
           .WithMany(u => u.Donations)
           .HasForeignKey(d => d.UserId)
           .OnDelete(DeleteBehavior.Cascade);
        
        
    }
}