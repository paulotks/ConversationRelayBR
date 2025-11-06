using System.Text.Json.Serialization;
namespace ConversartionRelayBR.Models.WebSocket.Incoming
{
    public abstract class BaseIncomingMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
