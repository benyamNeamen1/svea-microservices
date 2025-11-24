using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Svea.TimeTrackingService.Data;
using Svea.TimeTrackingService.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Svea.TimeTrackingService.Messaging;


namespace Svea.TimeTrackingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkController : ControllerBase
    {
        private readonly TimeDbContext _db;
        private readonly TimeTrackingRabbitMq _mq;
        public WorkController(TimeDbContext db, TimeTrackingRabbitMq mq)
            {
                _db = db;
                _mq = mq;
            }

        private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub));
        private Guid CurrentCompanyId => Guid.Parse(User.FindFirst("company_id")?.Value ?? throw new Exception("company_id missing"));

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn()
        {
            var userId = CurrentUserId;
            var companyId = CurrentCompanyId;

            bool valid = await _mq.ValidateUserAsync(CurrentUserId, CurrentCompanyId);
            if (!valid)
                return BadRequest("Invalid user. Timestamp not saved.");

            
            var open = await _db.WorkSessions.FirstOrDefaultAsync(w => w.UserId == userId && w.CheckOutUtc == null);
            if (open != null) return BadRequest(new { Information = "There is already an active session." });

            var ws = new WorkSession { UserId = userId, CompanyId = companyId, CheckInUtc = DateTime.UtcNow };
            _db.WorkSessions.Add(ws);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSession), new { id = ws.Id }, ws);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut()
        {
            bool valid = await _mq.ValidateUserAsync(CurrentUserId, CurrentCompanyId);
            if (!valid)
                return BadRequest("Invalid user. Timestamp not saved.");

            var userId = CurrentUserId;
            var open = await _db.WorkSessions.FirstOrDefaultAsync(w => w.UserId == userId && w.CheckOutUtc == null);
            if (open == null) return BadRequest(new { error = "No active session to check out." });

            open.CheckOutUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(open);
        }

        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            var userId = CurrentUserId;
            var sessions = await _db.WorkSessions.Where(w => w.UserId == userId).OrderByDescending(w => w.CheckInUtc).ToListAsync();
            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSession(Guid id)
        {
            var session = await _db.WorkSessions.FirstOrDefaultAsync(w => w.Id == id && w.CompanyId == CurrentCompanyId);
            if (session == null) return NotFound();
            return Ok(session);
        }
    }
}
