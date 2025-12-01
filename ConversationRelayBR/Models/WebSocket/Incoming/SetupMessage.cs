using System.Text.Json.Serialization;
namespace ConversartionRelayBR.Models.WebSocket.Incoming
{
    public class SetupMessage : BaseIncomingMessage
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;
        [JsonPropertyName("accountSid")]
        public string AccountSid { get; set; } = string.Empty;

        [JsonPropertyName("parentCallSid")]
        public string ParentCallSid { get; set; } = string.Empty;

        [JsonPropertyName("callSid")]
        public string CallSid { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        [JsonPropertyName("forwardedFrom")]
        public string ForwardedFrom { get; set; } = string.Empty;

        [JsonPropertyName("callType")]
        public string CallType { get; set; } = string.Empty;

        [JsonPropertyName("callerName")]
        public string CallerName { get; set; } = string.Empty;

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = string.Empty;

        [JsonPropertyName("callStatus")]
        public string CallStatus { get; set; } = string.Empty;

        [JsonPropertyName("customParameters")]
        public CustomParameters? CustomParameters { get; set; }
    }

    public class CustomParameters
    {
        [JsonPropertyName("callReference")]
        public string CallReference { get; set; } = string.Empty;
    }
}
