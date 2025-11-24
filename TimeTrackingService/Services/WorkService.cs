using Microsoft.EntityFrameworkCore;
using Svea.TimeTrackingService.Data;
using Svea.TimeTrackingService.Models;

namespace Svea.TimeTrackingService.Services
{
    public class WorkService : IWorkService
    {
        private readonly TimeDbContext _context;

        public WorkService(TimeDbContext context)
        {
            _context = context;
        }

        public async Task<WorkSession?> StartWorkAsync(Guid userId)
        {
            var activeSession = await _context.WorkSessions
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CheckOutUtc == null);

            if (activeSession != null)
            {
                return null; 
            }

            var newSession = new WorkSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CheckInUtc = DateTime.UtcNow
            };

            _context.WorkSessions.Add(newSession);
            await _context.SaveChangesAsync();
            return newSession;
        }

        public async Task<WorkSession?> EndWorkAsync(Guid userId)
        {
            var activeSession = await _context.WorkSessions
                .FirstOrDefaultAsync(w => w.UserId == userId && w.CheckOutUtc == null);

            if (activeSession == null)
            {
                return null; 
            }

            activeSession.CheckOutUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return activeSession;
        }

        public async Task<IEnumerable<WorkSession>> GetWorkSessionsAsync(Guid userId)
        {
            return await _context.WorkSessions
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CheckInUtc)
                .ToListAsync();
        }
    }
}
