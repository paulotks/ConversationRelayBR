using ConversartionRelayBR.Models.Enums;
using ConversartionRelayBR.Models.WebSocket.Incoming;
using ConversartionRelayBR.Models.WebSocket.Outgoing;
using ConversartionRelayBR.Services;
using Moq;
using System.Net.WebSockets;
using System.Reflection;

namespace ConversationRelayBR.test
{
    public class ConversationRelayServiceTests
    {
        private readonly Mock<WebSocketService> _mockWebSocketService;
        private readonly ConversationService _conversationService;
        private readonly Mock<WebSocket> _mockWebSocket;

        public ConversationRelayServiceTests()
        {
            _mockWebSocketService = new Mock<WebSocketService>();
            _conversationService = new ConversationService(_mockWebSocketService.Object);
            _mockWebSocket = new Mock<WebSocket>();
        }

        #region AnalyzeUserIntent Tests (CRÍTICO)

        [Theory]
        [InlineData("meu boleto venceu", IvrOption.BoletosVencidos)]
        [InlineData("preciso renegociar minha dívida", IvrOption.BoletosVencidos)]
        [InlineData("tenho débito em aberto", IvrOption.BoletosVencidos)]
        public void AnalyzeUserIntent_BoletosVencidos_ReturnsCorrectOption(string input, IvrOption expected)
        {
            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("sou cliente casas jardins", IvrOption.ClienteCasasJardins)]
        [InlineData("sobre minha casa", IvrOption.ClienteCasasJardins)]
        [InlineData("preciso agendar vistoria", IvrOption.ClienteCasasJardins)]
        public void AnalyzeUserIntent_ClienteCasasJardins_ReturnsCorrectOption(string input, IvrOption expected)
        {
            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("tenho uma dúvida", IvrOption.RelacionamentoCliente)]
        [InlineData("preciso de segunda via do contrato", IvrOption.RelacionamentoCliente)]
        [InlineData("quero fazer uma reclamação", IvrOption.RelacionamentoCliente)]
        [InlineData("extrato de pagamento", IvrOption.RelacionamentoCliente)]
        [InlineData("falar com atendente", IvrOption.RelacionamentoCliente)]
        [InlineData("quero falar com uma pessoa", IvrOption.RelacionamentoCliente)]
        [InlineData("operador humano", IvrOption.RelacionamentoCliente)]
        public void AnalyzeUserIntent_RelacionamentoCliente_ReturnsCorrectOption(string input, IvrOption expected)
        {
            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("quero comprar um lote", IvrOption.StandVendas)]
        [InlineData("falar com corretor", IvrOption.StandVendas)]
        [InlineData("informações sobre vendas", IvrOption.StandVendas)]
        public void AnalyzeUserIntent_StandVendas_ReturnsCorrectOption(string input, IvrOption expected)
        {
            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("tenho problema elétrico", IvrOption.AssistenciaTecnica)]
        [InlineData("preciso abrir chamado", IvrOption.AssistenciaTecnica)]
        [InlineData("vazamento na casa", IvrOption.AssistenciaTecnica)]
        public void AnalyzeUserIntent_AssistenciaTecnica_ReturnsCorrectOption(string input, IvrOption expected)
        {
            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Equal(expected, result);
        }



        [Theory]
        [InlineData("opção um", IvrOption.BoletosVencidos)]
        [InlineData("número dois", IvrOption.ClienteCasasJardins)]
        [InlineData("três", IvrOption.RelacionamentoCliente)]
        [InlineData("quatro", IvrOption.StandVendas)]
        [InlineData("cinco", IvrOption.AssistenciaTecnica)]
        public async Task AnalyzeUserIntent_NumericWords_WhenInDTMFMenu_ReturnsCorrectOption(string input, IvrOption expected)
        {
            var setupMessage = new SetupMessage
            {
                CallSid = "test-call-sid",
                SessionId = "test-session-id",
                From = "+5511999999999",
                To = "+551133334444"
            };

            _mockWebSocketService
                .Setup(x => x.SendMessageAsync(It.IsAny<WebSocket>(), It.IsAny<TextMessage>()))
                .Returns(Task.CompletedTask);

            await InvokePrivateMethodAsync("HandleSetupAsync", _mockWebSocket.Object, setupMessage);

            var unrecognizedPrompt = new PromptMessage
            {
                Text = "blablabla não reconhecido",
                Language = "pt-BR",
                Last = true
            };

            // forcar duas chamadas para simular o fluxo até o menu DTMF
            await InvokePrivateMethodAsync("HandlePromptAsync", _mockWebSocket.Object, unrecognizedPrompt);
            await InvokePrivateMethodAsync("HandlePromptAsync", _mockWebSocket.Object, unrecognizedPrompt);


            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("opção um")]
        [InlineData("número dois")]
        [InlineData("três")]
        public void AnalyzeUserIntent_NumericWords_WhenNotInDTMFMenu_ReturnsNull(string input)
        {

            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Null(result);
        }

        //caso solicitem implementaçao de transferência para humano separada da de relacionamento cliente
        //[Theory]
        //[InlineData("falar com atendente", IvrOption.Recepcao)]
        //[InlineData("quero falar com uma pessoa", IvrOption.Recepcao)]
        //[InlineData("operador humano", IvrOption.Recepcao)]
        //public void AnalyzeUserIntent_TransferToHuman_ReturnsRecepcao(string input, IvrOption expected)
        //{
        //    // Act
        //    var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

        //    // Assert
        //    Assert.Equal(expected, result);
        //}

        [Theory]
        [InlineData("blablabla")]
        [InlineData("xpto123")]
        [InlineData("não entendi nada")]
        public void AnalyzeUserIntent_UnrecognizedInput_ReturnsNull(string input)
        {
            // Act
            var result = InvokePrivateMethod<IvrOption?>("AnalyzeUserIntent", input);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region DTMF Tests (CRÍTICO)

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        public async Task HandleDtmfAsync_ValidDigits_CallsProcessValidIntent(string digit)
        {
            // Arrange
            var dtmfMessage = new DtmfMessage { Digit = digit };

            _mockWebSocketService
                .Setup(x => x.SendMessageAsync(It.IsAny<WebSocket>(), It.IsAny<TextMessage>()))
                .Returns(Task.CompletedTask);

            _mockWebSocketService
                .Setup(x => x.EndSessionMessageAsync(It.IsAny<WebSocket>(), It.IsAny<EndMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            await InvokePrivateMethodAsync("HandleDtmfAsync", _mockWebSocket.Object, dtmfMessage);

            // Assert - Verifica que EndSessionMessageAsync foi chamado (indica sucesso)
            _mockWebSocketService.Verify(
                x => x.EndSessionMessageAsync(It.IsAny<WebSocket>(), It.IsAny<EndMessage>()),
                Times.Once
            );
        }

        [Theory]
        [InlineData("0")]
        [InlineData("7")]
        [InlineData("9")]
        [InlineData("*")]
        [InlineData("#")]
        public async Task HandleDtmfAsync_InvalidDigits_SendsErrorMessage(string digit)
        {
            // Arrange
            var dtmfMessage = new DtmfMessage { Digit = digit };

            // Act
            await InvokePrivateMethodAsync("HandleDtmfAsync", _mockWebSocket.Object, dtmfMessage);

            // Assert
            _mockWebSocketService.Verify(
                x => x.SendMessageAsync(
                    It.IsAny<WebSocket>(),
                    It.Is<TextMessage>(m => m.Token.Contains("Opção inválida"))
                ),
                Times.Once
            );
        }

        #endregion

        #region Helper Methods

        private T? InvokePrivateMethod<T>(string methodName, params object[] parameters)
        {
            var method = typeof(ConversationService).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            return (T?)method?.Invoke(_conversationService, parameters);
        }

        private async Task InvokePrivateMethodAsync(string methodName, params object[] parameters)
        {
            var method = typeof(ConversationService).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var task = (Task?)method?.Invoke(_conversationService, parameters);
            if (task != null)
            {
                await task;
            }
        }

        #endregion
    }
}
