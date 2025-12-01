using System.Text.Json.Serialization;
namespace ConversartionRelayBR.Models.WebSocket.Incoming
{
    public class PromptMessage : BaseIncomingMessage
    {
        [JsonPropertyName("voicePrompt")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("lang")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("last")]
        public bool Last { get; set; }
    }
}
