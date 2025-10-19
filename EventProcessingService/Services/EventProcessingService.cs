namespace EventProcessingService.Services;

public class EventProcessingService : IHostedService
{
    private readonly IEventObservable _eventObservable;
    private readonly EventObserver _eventObserver;
    private IDisposable? _subscription;
    private readonly ILogger<EventProcessingService> _logger;

    public EventProcessingService(
        IEventObservable eventObservable,
        EventObserver eventObserver,
        ILogger<EventProcessingService> logger)
    {
        _eventObservable = eventObservable;
        _eventObserver = eventObserver;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Запуск сервиса обработки событий");
        
        await _eventObserver.InitializeAsync();
        _subscription = _eventObservable.Subscribe(_eventObserver);
        
        _logger.LogInformation("Сервис обработки событий запущен");
    }

    public Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation("Остановка сервиса обработки событий");
        
        _subscription?.Dispose();

        _logger.LogInformation("Сервис обработки событий остановлен");
        
        return Task.CompletedTask;
    }
}