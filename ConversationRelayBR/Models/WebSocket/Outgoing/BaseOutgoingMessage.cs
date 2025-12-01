using System.Text.Json.Serialization;
namespace ConversartionRelayBR.Models.WebSocket.Outgoing
{
    public abstract class BaseOutgoingMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
