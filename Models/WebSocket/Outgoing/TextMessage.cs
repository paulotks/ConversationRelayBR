using System.Text.Json.Serialization;

namespace ConversartionRelayBR.Models.WebSocket.Outgoing
{
    public class TextMessage : BaseOutgoingMessage
    {
        public TextMessage()
        {
            Type = "text";
        }

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
        [JsonPropertyName("lang")]
        public string? Language { get; set; }
        [JsonPropertyName("last")]
        public bool? Last { get; set; }
        [JsonPropertyName("interruptible")]
        public bool? Interruptible { get; set; }
        [JsonPropertyName("preemptible")]
        public bool? Preemptible { get; set; }
    }
}
