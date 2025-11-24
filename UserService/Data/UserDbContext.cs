using Microsoft.EntityFrameworkCore;
using Svea.UserService.Models;

namespace Svea.UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> opts) : base(opts) { }

        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<Company>().HasKey(c => c.Id);
            mb.Entity<User>().HasKey(u => u.Id);
            mb.Entity<User>()
              .HasIndex(u => new { u.Email, u.CompanyId })
              .IsUnique();
            mb.Entity<User>()
              .HasOne(u => u.Company)
              .WithMany(c => c.Users)
              .HasForeignKey(u => u.CompanyId);
        }
    }
}
