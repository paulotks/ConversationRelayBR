using System.Text.Json.Serialization;

namespace ConversartionRelayBR.Models.WebSocket.Outgoing
{
    public class EndMessage : BaseOutgoingMessage
    {
        public EndMessage()
        {
            Type = "end";
        }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}
