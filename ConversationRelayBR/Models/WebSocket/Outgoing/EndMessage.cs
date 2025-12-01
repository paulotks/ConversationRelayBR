using System.Text.Json.Serialization;

namespace ConversartionRelayBR.Models.WebSocket.Outgoing
{
    public class EndMessage : BaseOutgoingMessage
    {
        public EndMessage()
        {
            Type = "end";
        }

        [JsonPropertyName("handoffData")]
        public string? Reason { get; set; }
    }
}
