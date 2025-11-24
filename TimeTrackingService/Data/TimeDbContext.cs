using Microsoft.EntityFrameworkCore;
using Svea.TimeTrackingService.Models;

namespace Svea.TimeTrackingService.Data
{
    public class TimeDbContext : DbContext
    {
        public TimeDbContext(DbContextOptions<TimeDbContext> opts) : base(opts) { }
        public DbSet<WorkSession> WorkSessions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<WorkSession>().HasKey(w => w.Id);
            mb.Entity<WorkSession>().HasIndex(w => new { w.UserId, w.CheckInUtc });
        }
    }
}
