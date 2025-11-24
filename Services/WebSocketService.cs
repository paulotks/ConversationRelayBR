using ConversartionRelayBR.Models.WebSocket.Incoming;
using ConversartionRelayBR.Models.WebSocket.Outgoing;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ConversartionRelayBR.Services
{
    public class WebSocketService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public WebSocketService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public BaseIncomingMessage? DesserializeIncomingMessage(string jsonMessage)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonMessage);
                var type = document.RootElement.GetProperty("type").GetString();

                return type switch
                {
                    "setup" => JsonSerializer.Deserialize<SetupMessage>(jsonMessage, _jsonOptions),
                    "prompt" => JsonSerializer.Deserialize<PromptMessage>(jsonMessage, _jsonOptions),
                    "dtmf" => JsonSerializer.Deserialize<DtmfMessage>(jsonMessage, _jsonOptions),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deserializar mensagem: {ex.Message}");
                return null;
            }
        }

        public string SerializeOutGoingMessage(TextMessage message)
        {
            return JsonSerializer.Serialize(message, _jsonOptions);
        }

        public string SerializeEndSessionMessage(EndMessage message)
        {
            return JsonSerializer.Serialize(message, _jsonOptions);
        }

        public async Task SendMessageAsync(WebSocket webSocket, TextMessage message)
        {
            var json = SerializeOutGoingMessage(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
                );

            Console.WriteLine($"Mensagem enviada: {json}");
        }
        public async Task EndSessionMessageAsync(WebSocket webSocket, EndMessage endMessage)
        {
            var json = SerializeEndSessionMessage(endMessage);
            var bytes = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
                );

            Console.WriteLine($"Mensagem de Encerramento enviada enviada: {json}");
        }
    }
}
