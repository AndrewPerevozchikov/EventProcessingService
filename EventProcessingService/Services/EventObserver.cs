using EventProcessingService.Data;
using EventProcessingService.Models;

namespace EventProcessingService.Services;

public class EventObserver : IObserver<UserEvent>
    {
        private readonly IDataStorage _dataStorage;
        private readonly ILogger<EventObserver> _logger;
        private readonly Dictionary<(int UserId, string EventType), int> _eventCounts;

        public EventObserver(IDataStorage dataStorage, ILogger<EventObserver> logger)
        {
            _dataStorage = dataStorage;
            _logger = logger;
            _eventCounts = new Dictionary<(int, string), int>();
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                var statistics = await _dataStorage.GetStatistics();
                var loadedCounts  = statistics.ToDictionary(x => (x.UserId, x.EventType), x => x.Count);

                foreach (var (key, count) in loadedCounts)
                {
                    _eventCounts[key] = count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при инициализации EventObserver");
            }
        }

        public void OnNext(UserEvent value)
        {
            try
            {
                var key = (value.UserId, value.EventType);
                
                if (!_eventCounts.TryAdd(key, 1))
                {
                    _eventCounts[key]++;
                }

                _logger.LogInformation("Обработано событие: UserId={UserId}, EventType={EventType}, CurrentCount={Count}", 
                    value.UserId, value.EventType, _eventCounts[key]);

                SaveStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке события для UserId={UserId}", value.UserId);
            }
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "Произошла ошибка в EventObserver");
        }

        public void OnCompleted()
        {
            _logger.LogInformation("EventObserver завершил работу");
            SaveStatistics();
        }

        private void SaveStatistics()
        {
            try
            {
                var stats = _eventCounts.Select(kvp => new UserEventStats
                {
                    UserId = kvp.Key.UserId,
                    EventType = kvp.Key.EventType,
                    Count = kvp.Value
                }).ToList();

                _dataStorage.SaveStatistics(stats);
                _logger.LogDebug("Статистика сохранена: {Count} записей", stats.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении статистики");
            }
        }
        
        public Dictionary<(int, string), int> GetCurrentCounts()
        {
            return new Dictionary<(int, string), int>(_eventCounts);
        }
    }