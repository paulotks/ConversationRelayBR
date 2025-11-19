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

            StartWaitTimer(webSocket, 19000); //8 segundos

            return Task.CompletedTask;
        }

        private async Task HandlePromptAsync(WebSocket webSocket, PromptMessage prompt)
        {
            Console.WriteLine($"Cliente Falou: '{prompt.Text}' (Idioma: {prompt.Language})");

            if (string.IsNullOrWhiteSpace(prompt.Text) || prompt.Text.Trim().Length < 3)
            {
                return; // NÃO cancela timer
            }

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

            // Opção 1: Boletos Vencidos (Financeiro/Cobrança)
            if (ContainsAny(input, "boleto", "vencido","em aberto", "negociar", "renegociar", "parcelar", "vencimento", "pagamento", "pagar", "débito", "dívida", "cobrança", "financeiro", "extrato", "fatura"))
                return IvrOption.BoletosVencidos;

            // Opção 2: Cliente Casas Jardins
            if (ContainsAny(input, "casas jardins", "casa jardim", "meu empreendimento", "minha casa", "meu imóvel", "entrega", "iptu", "vistoria", "visita"))
                return IvrOption.ClienteCasasJardins;

            // Opção 3: Relacionamento com Cliente (Atendimento geral, dúvidas, extratos a vencer)
            if (ContainsAny(input, "relacionamento", "atendimento", "atender", "cliente", "dúvida", "informação", "reclamação", "sugestão", "elogio", "boleto a vencer", "extrato", "segunda via", "contrato", "documentação", "projeto", "lote", "balão"))
                return IvrOption.RelacionamentoCliente;

            // Opção 4: Stande de Vendas (Comercial/Comprar)
            if (ContainsAny(input, "comprar", "compra", "vendas", "venda", "comercial", "corretor", "stand", "stande", "adquirir", "aquisição", "interesse", "novo", "lançamento"))
                return IvrOption.StandeVendas;

            // Opção 5: Assistência Técnica (Pós-entrega - Problemas/Manutenção)
            if (ContainsAny(input, "assistência", "chamado", "agendamento", "projetos", "danificado", "reforma", "elétrica", "técnica", "problema", "defeito", "manutenção", "reparo", "conserto", "quebrado", "vazamento", "infiltração", "pós-entrega", "pós entrega"))
                return IvrOption.AssistenciaTecnica;

            // Palavras-chave numéricas (caso o usuário fale o número)
            if (ContainsAny(input, "um", "número um", "opção um", "1"))
                return IvrOption.BoletosVencidos;

            if (ContainsAny(input, "dois", "número dois", "opção dois", "2"))
                return IvrOption.ClienteCasasJardins;

            if (ContainsAny(input, "três", "número três", "opção três", "3"))
                return IvrOption.RelacionamentoCliente;

            if (ContainsAny(input, "quatro", "número quatro", "opção quatro", "4"))
                return IvrOption.StandeVendas;

            if (ContainsAny(input, "cinco", "número cinco", "opção cinco", "5"))
                return IvrOption.AssistenciaTecnica;

            // Transferência para atendente humano
            if (ContainsAny(input, "atendente", "humano", "pessoa", "falar com alguém", "operador", "recepcão", "recepção"))
                return IvrOption.Recepcao;

            return null;
        }

        private static bool ContainsAny(string input, params string[] keywords)
        {
            return keywords.Any(keyword => input.Contains(keyword));
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
                _ => "Transferindo. Aguarde."
            };
            var responseMessage = new TextMessage { Token = message };
            await _webSocketService.SendMessageAsync(webSocket, responseMessage);

            await Task.Delay(1500);


            var endMessage = new EndMessage
            {
                Reason = ((int)intent).ToString() // "1", "2", "3", etc
            };

            Console.WriteLine($"Encerrando sessão com motivo: {endMessage.Reason}");

            await _webSocketService.EndSessionMessageAsync(webSocket, endMessage);
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
                Token = "Por favor escolha uma das opções a seguir pressionando o número correspondente:" +
                " Um para boletos vencidos." +
                " Dois para informações sobre o seu empreendimento casas jardins." +
                " Três para relacionamento com o cliente incluindo boletos a vencer e extratos de pagamento." +
                " Quatro para o setor comercial para falar com nosso stand de vendas ou com um corretor." +
                " Cinco para assistência técnica. " +
                " Se nenhuma dessas opções atender à sua necessidade, aguarde. Em instantes, um de nossos atendentes falará com você." +
                "Ao longo de 36 anos, a FGR se tornou especialista em condomínios horizontais, e agora, com o projeto da construção de Casas nos Jardins, criamos o melhor lugar para as pessoas serem felizes. Para obter mais informações, acesse nosso site ou aplicativo FGR Jardins. FGR Incorporações, mudando vidas."
            };

            await _webSocketService.SendMessageAsync(webSocket, optionsMessage);
            _currentState = CallFlowState.WaitingDTMF;

            StartWaitTimer(webSocket, 45000);
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
            switch (_currentState)
            {
                case CallFlowState.Initial:
                    await ShowOptionsAsync(webSocket);
                    break;

                case CallFlowState.WaitingDTMF:
                    await ProcessValidIntentAsync(webSocket, IvrOption.Recepcao);
                    break;

                default:
                    await ProcessValidIntentAsync(webSocket, IvrOption.Recepcao);
                    break;
            }
        }
    }
}
