using EventProcessingService.Data;
using EventProcessingService.Models;
using EventProcessingService.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestEventProcessing;

public class EventObserverTests
{
    private readonly Mock<ILogger<EventObserver>> _mockLogger;
    private readonly EventObserver _eventObserver;

    public EventObserverTests()
    {
        var mockDataStorage = new Mock<IDataStorage>();
        _mockLogger = new Mock<ILogger<EventObserver>>();
        _eventObserver = new EventObserver(mockDataStorage.Object, _mockLogger.Object);
    }

    [Fact]
    public void OnNext_IncrementsEventCount()
    {
        var userEvent = new UserEvent { UserId = 123, EventType = "click" };

        _eventObserver.OnNext(userEvent);
        _eventObserver.OnNext(userEvent);

        var counts = _eventObserver.GetCurrentCounts();
        var key = (123, "click");
        Assert.True(counts.ContainsKey(key));
        Assert.Equal(2, counts[key]);
    }

    [Fact]
    public void OnNext_DifferentEvents_CountedSeparately()
    {
        var clickEvent = new UserEvent { UserId = 123, EventType = "click" };
        var hoverEvent = new UserEvent { UserId = 123, EventType = "hover" };

        _eventObserver.OnNext(clickEvent);
        _eventObserver.OnNext(hoverEvent);
        _eventObserver.OnNext(clickEvent);

        var counts = _eventObserver.GetCurrentCounts();
        Assert.Equal(2, counts[(123, "click")]);
        Assert.Equal(1, counts[(123, "hover")]);
    }
}