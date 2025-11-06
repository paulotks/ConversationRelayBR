using ConversartionRelayBR.Models.WebSocket.Incoming;
using ConversartionRelayBR.Models.WebSocket.Outgoing;
using System.Net.WebSockets;

namespace ConversartionRelayBR.Services
{
    public class ConversationService(WebSocketService webSocketService)
    {
        private readonly WebSocketService _webSocketService = webSocketService;
        private string? _sessionId;
        private string? _callSid;

        //public ConversationService(WebSocketService webSocketService)
        //{
        //    _webSocketService = webSocketService;
        //}

        public async Task ProcessMessageAsync(WebSocket webSocket, string jsonMessage)
        {
            var incomingMessage = _webSocketService.DesserializeIncomingMessage(jsonMessage);

            if (incomingMessage == null)
            {
                Console.WriteLine("Mensagem não reconhecida.");
                return;
            }

            switch (incomingMessage)
            {
                case SetupMessage setup:
                    await HandleSetupAsync(webSocket, setup);
                    break;
                case PromptMessage prompt:
                    await HandlePromptAsync(webSocket, prompt);
                    break;
                default:
                    Console.WriteLine($"Tipo de mensagem não implementado: {incomingMessage.Type}");
                    break;
            }
        }

        private async Task HandleSetupAsync(WebSocket webSocket, SetupMessage setup)
        {
            _callSid = setup.CallSid;
            _sessionId = setup.SessionId;
            Console.WriteLine($"Nova chamada conectada - CallSid: {setup.CallSid}");
            Console.WriteLine($"De: {setup.From} Para: {setup.To}");

            var welcomeMessage = new TextMessage {
                Token = "Olá! Você está conectado ao nosso sistema de atendimento. Como posso ajudá-lo hoje?",
            };

            await _webSocketService.SendMessageAsync(webSocket, welcomeMessage);
        }

        private async Task HandlePromptAsync(WebSocket webSocket, PromptMessage prompt)
        {
            Console.WriteLine($"Cliente Falou: '{prompt.Text}' (Idioma: {prompt.Language})");

            var responseText = GenerateResponse(prompt.Text);

            var responseMessage = new TextMessage
            {
                Token = responseText,
            };

            await _webSocketService.SendMessageAsync(webSocket, responseMessage);
        }

        private static string GenerateResponse(string userInput)
        {
            var input = userInput.ToLowerInvariant();

            return input switch
            {
                var text when text.Contains("saldo") => "Para consultar seu saldo, preciso verificar algumas informações. Qual é o seu CPF?",
                var text when text.Contains("cartão") => "Entendi que você tem uma questão sobre cartão. Pode me dar mais detalhes sobre o problema?",
                var text when text.Contains("empréstimo") => "Para informações sobre empréstimos, vou te transferir para um especialista. Aguarde um momento.",
                var text when text.Contains("tchau") || text.Contains("obrigado") => "Foi um prazer atendê-lo! Tenha um ótimo dia!",
                _ => "Entendi. Pode repetir ou me dar mais detalhes sobre sua solicitação?"
            };
        }
    }
}
