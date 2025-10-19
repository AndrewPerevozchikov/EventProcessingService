using System.Text.Json;
using EventProcessingService.Models;

namespace EventProcessingService.Data;

public class FileDataStorage : IDataStorage
{
    private readonly string _filePath;
    private readonly ILogger<FileDataStorage> _logger;

    public FileDataStorage(IConfiguration configuration, ILogger<FileDataStorage> logger)
    {
        _logger = logger;
        _filePath = configuration["FILE_STORAGE_PATH"] ?? "user_event_stats.json";
    }

    public async Task SaveStatistics(List<UserEventStats> statistics)
    {
        try
        {
            var json = JsonSerializer.Serialize(statistics, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_filePath, json);
            _logger.LogDebug("Статистика сохранена в файл: {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении в файл");
            throw;
        }
    }

    public async Task<List<UserEventStats>> GetStatistics()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<UserEventStats>>(json)
                   ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при чтении из файла");
            return [];
        }
    }
}