

namespace Svea.TimeTrackingService.Models
{
    public class WorkSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CheckInUtc { get; set; }
        public DateTime? CheckOutUtc { get; set; }
        public TimeSpan? Duration => CheckOutUtc.HasValue ? CheckOutUtc - CheckInUtc : null;
    }
}
