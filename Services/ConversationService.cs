using ConversartionRelayBR.Models.Enums;
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
        private CallFlowState _currentState = CallFlowState.Initial;
        private int _attemptCount = 0;
        private Timer? _waitTimer;


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
                case DtmfMessage dtmf:
                    await HandleDtmfAsync(webSocket, dtmf);
                    break;
                default:
                    Console.WriteLine($"Tipo de mensagem não implementado: {incomingMessage.Type}");
                    break;
            }
        }

        private Task HandleSetupAsync(WebSocket webSocket, SetupMessage setup)
        {
            _callSid = setup.CallSid;
            _sessionId = setup.SessionId;
            Console.WriteLine($"Nova chamada conectada - CallSid: {setup.CallSid}");
            Console.WriteLine($"De: {setup.From} Para: {setup.To}");
            Console.WriteLine($"Iniciando timer de 8 segundos - Estado: {_currentState}");

            StartWaitTimer(webSocket, 20000); //8 segundos

            return Task.CompletedTask;
        }

        private async Task HandlePromptAsync(WebSocket webSocket, PromptMessage prompt)
        {
            Console.WriteLine($"Cliente Falou: '{prompt.Text}' (Idioma: {prompt.Language})");

            _waitTimer?.Dispose();

            var intent = AnalyzeUserIntent(prompt.Text);

            if (intent != null)
            {
                await ProcessValidIntentAsync(webSocket, intent.Value);
            } else
            {
                await HandleUnrecognizedInputAsync(webSocket);
            }

        }

        private async Task HandleDtmfAsync(WebSocket webSocket, DtmfMessage dtmf)
        {
            Console.WriteLine($"Cliente pressionou a tecla: {dtmf.Digit}");

            if (int.TryParse(dtmf.Digit, out int digit) && Enum.IsDefined(typeof(IvrOption), digit))
            {
                var option = (IvrOption)int.Parse(dtmf.Digit);
                await ProcessValidIntentAsync(webSocket, option);
            }
            else
            {
                var responseMessage = new TextMessage
                {
                    Token = "Opção inválida. Por favor, pressione uma tecla de 1 a 5."
                };
                await _webSocketService.SendMessageAsync(webSocket, responseMessage);
            }
        }
        private IvrOption? AnalyzeUserIntent(string userInput)
        {
            var input = userInput.ToLowerInvariant();

            return input switch
            {
                var text when text.Contains("extrato") || text.Contains("boleto") || text.Contains("pagamento")
                    => IvrOption.BoletosVencidos,
                var text when text.Contains("relacionamento") || text.Contains("cliente") || text.Contains("reclamação")
                    => IvrOption.RelacionamentoCliente,
                var text when text.Contains("comercial") || text.Contains("vendas") || text.Contains("comprar")
                    => IvrOption.StandeVendas,
                var text when text.Contains("assistência") || text.Contains("técnica") || text.Contains("problema") || text.Contains("manutenção")
                    => IvrOption.AssistenciaTecnica,
                var text when text.Contains("dúvida") || text.Contains("empreendimento") || text.Contains("informação") || text.Contains("projeto")
                    => IvrOption.RelacionamentoCliente,
                _ => null
            };
        }

        private async Task ProcessValidIntentAsync(WebSocket webSocket, IvrOption intent)
        {
            var message = intent switch
            {
                IvrOption.BoletosVencidos => "Vou conectá-lo ao setor financeiro para questões sobre extrato e boletos. Aguarde um momento.",
                IvrOption.RelacionamentoCliente => "Transferindo para o relacionamento com o cliente. Um momento, por favor.",
                IvrOption.StandeVendas => "Conectando com nossa equipe comercial. Aguarde.",
                IvrOption.AssistenciaTecnica => "Direcionando para assistência técnica. Um momento.",
                IvrOption.ClienteCasasJardins => "Transferindo para o relacionamento com o cliente. Um momento, por favor.",
            };
            var responseMessage = new TextMessage { Token = message };
            await _webSocketService.SendMessageAsync(webSocket, responseMessage);

            // Simular transferência e finalizar

        }

        private async Task HandleUnrecognizedInputAsync(WebSocket webSocket)
        {
            _attemptCount++;
            if (_attemptCount == 1)
            {
                _currentState = CallFlowState.SecondChance;
                var responseMessage = new TextMessage
                {
                    Token = "Desculpe, não consegui entender. Pode repetir o motivo do seu contato?",
                };
                await _webSocketService.SendMessageAsync(webSocket, responseMessage);
                StartWaitTimer(webSocket, 12000); //12 segundos
            } else
            {
                _currentState = CallFlowState.ShowingOptions;
                await ShowOptionsAsync(webSocket);
            }
        }

        private async Task ShowOptionsAsync(WebSocket webSocket)
        {
            var optionsMessage = new TextMessage
            {
                Token = "Por favor escolha uma das seguintes opções pressionando o número correspondente: " +
                                "Pressione 1 para extrato e boletos, " +
                                "2 para relacionamento com o cliente, " +
                                "3 para comercial, " +
                                "4 para assistência técnica, " +
                                "ou 5 para dúvidas sobre empreendimentos."
            };

            await _webSocketService.SendMessageAsync(webSocket, optionsMessage);
            _currentState = CallFlowState.WaitingDTMF;
        }

        private void StartWaitTimer(WebSocket webSocket, int millisecondsDelay)
        {
            _waitTimer = new Timer(_ =>
            {
                _ = Task.Run(async () => await OnWaitTimeoutAsync(webSocket));
            }, null, millisecondsDelay, Timeout.Infinite);
        }

        private async Task OnWaitTimeoutAsync(WebSocket webSocket)
        {
            Console.WriteLine($"TIMEOUT Estado: {_currentState}, Tentativas: {_attemptCount}");
            
            var responseMessage = new TextMessage
            {
                Token = "Não consegui entender sua solicitação.",
            };

            await _webSocketService.SendMessageAsync(webSocket, responseMessage);
            await ShowOptionsAsync(webSocket);
        }
    }
}
