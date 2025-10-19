using EventProcessingService.Data;
using EventProcessingService.Services;

namespace EventProcessingService.Extensions;

public static class ServiceCollectionsExtensions
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEventObservable, EventObservable>();
        builder.Services.AddSingleton<EventObserver>();
        builder.Services.AddSingleton<IDataStorage, FileDataStorage>();
        builder.Services.AddHostedService<KafkaConsumer>();
        builder.Services.AddHostedService<EventProcessingService.Services.EventProcessingService>();
        
        return builder;
    }
}