using EventProcessingService.Models;

namespace EventProcessingService.Data;

public interface IDataStorage
{
    Task SaveStatistics(List<UserEventStats> statistics);
    Task<List<UserEventStats>> GetStatistics();
}