using System.Text.Json.Serialization;

namespace EventProcessingService.Models;

public class UserEventStats
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("eventType")] 
    public string EventType { get; set; } = string.Empty;
        
    [JsonPropertyName("count")]
    public int Count { get; set; }
}