using Svea.TimeTrackingService.Models;

namespace Svea.TimeTrackingService.Services
{
    public interface IWorkService
    {
        Task<WorkSession?> StartWorkAsync(Guid userId);
        Task<WorkSession?> EndWorkAsync(Guid userId);
        Task<IEnumerable<WorkSession>> GetWorkSessionsAsync(Guid userId);
    }
}
