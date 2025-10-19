using EventProcessingService.Models;

namespace EventProcessingService.Services;

public interface IEventObservable : IObservable<UserEvent>
{
    void PublishEvent(UserEvent userEvent);
    IObservable<UserEvent> AsObservable();
}