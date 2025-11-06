using System.Text.Json.Serialization;

namespace ConversartionRelayBR.Models.WebSocket.Incoming
{
    public class DtmfMessage : BaseIncomingMessage
    {
        [JsonPropertyName("digit")]
        public string Digit { get; set; } = string.Empty;
    }
}
