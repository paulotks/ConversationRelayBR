namespace ConversartionRelayBR.Models.WebSocket.TwilioSettings
{
    public class TwilioSettings
    {
        public string TwilioAccountSid { get; set; } = string.Empty;
        public string TwilioAuthToken { get; set; } = string.Empty;
        public string TwilioApiSid { get; set; } = string.Empty;
        public string TwilioApiSecret { get; set; } = string.Empty;
        public string SoftPhoneUrl { get; set; } = string.Empty;
        public string StandPhoneNumber { get; set; } = string.Empty;
        public string WebSocketUrl { get; set; } = string.Empty;

        public bool RequestValidationEnabled { get; set; } = true;
    }
}
