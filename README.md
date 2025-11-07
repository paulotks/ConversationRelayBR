# ConversationRelay BR

Sistema de atendimento telefônico inteligente que integra com a Twilio para fornecer IVR (Interactive Voice Response) com reconhecimento de voz e processamento de intenções.

## Funcionalidades

- **Reconhecimento de Voz**: Processa entrada de voz em português brasileiro
- **Suporte DTMF**: Aceita entrada via teclado telefônico (1-5)
- **Análise de Intenções**: Identifica automaticamente o que o cliente deseja
- **Sistema de Timeout**: Gerencia tempos de espera inteligentes
- **Roteamento Automático**: Direciona para o setor correto

## Tecnologias

- .NET 9 com C# 13.0
- ASP.NET Core WebAPI
- Twilio SDK
- WebSockets
- System.Text.Json

## Configuração

### Pré-requisitos
- .NET 9 SDK
- Conta Twilio ativa

### Instalação

1. Clone o repositório: git clone https://github.com/paulotks/ConversationRelayBR.git cd ConversationRelayBR
2. Configure as credenciais da Twilio no `appsettings.json`: { "TWILIO_ACCOUNT_SID": "seu_account_sid", "TWILIO_AUTH_TOKEN": "seu_auth_token" } //nao precisa ainda
3. Execute o projeto: dotnet run


## Como Funciona

### Fluxo de Atendimento
1. Cliente liga e é conectado via WebSocket
2. Sistema reproduz mensagem de boas-vindas
3. Aguarda resposta do cliente (20s)
4. Analisa a fala para identificar intenção
5. Se não entender, oferece segunda chance (15s)
6. Apresenta menu DTMF com opções numeradas
7. Direciona para setor apropriado

### Opções do IVR
- **1** - Boletos Vencidos / Financeiro
- **2** - Cliente Casas Jardins
- **3** - Relacionamento com Cliente  
- **4** - Stande de Vendas / Comercial
- **5** - Assistência Técnica

### Palavras-Chave Reconhecidas
- **Financeiro**: extrato, boleto, pagamento
- **Relacionamento**: cliente, reclamação, dúvida
- **Comercial**: vendas, comprar, comercial
- **Técnica**: assistência, problema, manutenção

## Estrutura do Projeto
project-root/
├── Controllers/
│   └── IncomingCallHTTP.cs     # Webhook
├── Services/
│   ├── WebSocketService.cs     # Gerencia WebSocket
│   └── ConversationService.cs  # Lógica de conversa
├── Models/
│   ├── Enums/
│   │   ├── CallFlowState.cs    # Estados do fluxo
│   │   └── IvrOptions.cs       # Opções do menu
│   └── WebSocket/
│       ├── Incoming/           # Mensagens recebidas
│       └── Outgoing/           # Mensagens enviadas
└── Program.cs                  # WebSocket

## Endpoints

- **POST /** - Webhook para chamadas Twilio
- **WS /websocket** - Conexão WebSocket para mensagens

## Licença
Paulin fica ligeiro
