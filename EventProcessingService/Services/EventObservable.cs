using System.Reactive.Linq;
using System.Reactive.Subjects;
using EventProcessingService.Models;

namespace EventProcessingService.Services
{
    public class EventObservable : IEventObservable, IDisposable
    {
        private readonly Subject<UserEvent> _subject = new();
        private bool _disposed;

        public void PublishEvent(UserEvent userEvent)
        {
            if (!_disposed)
            {
                _subject.OnNext(userEvent);
            }
        }

        public IObservable<UserEvent> AsObservable()
        {
            return _subject.AsObservable();
        }

        public IDisposable Subscribe(IObserver<UserEvent> observer)
        {
            return _subject.Subscribe(observer);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _subject.OnCompleted();
                _subject.Dispose();
                _disposed = true;
            }
        }
    }
}