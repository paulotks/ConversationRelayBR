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
- URL pública acessível (ngrok ou servidor)

### Instalação

1. Clone o repositório: git clone https://github.com/paulotks/ConversationRelayBR.git cd ConversationRelayBR
2. Configure as credenciais da Twilio no `appsettings.json`: { "TWILIO_ACCOUNT_SID": "seu_account_sid", "TWILIO_AUTH_TOKEN": "seu_auth_token" } //nao precisa ainda
3. Execute o projeto: dotnet run


## Como Funciona

### Fluxo de Atendimento
1. Cliente liga e é conectado via WebSocket
2. Sistema reproduz mensagem de boas-vindas (19s de espera)
3. Analisa a fala para identificar intenção automaticamente
4. Se não entender, oferece segunda chance (12s de espera)
5. Apresenta menu DTMF completo com opções numeradas (45s de espera)
6. Cliente escolhe via voz ou teclado
7. Sistema transfere para o setor apropriado ou telefone direto

### Opções do IVR
- **1** - Boletos Vencidos / Financeiro
- **2** - Cliente Casas Jardins
- **3** - Relacionamento com Cliente  
- **4** - Stande de Vendas / Comercial
- **5** - Assistência Técnica
- **Timeout** - Transfere automaticamente para recepção

### Palavras-Chave Reconhecidas
**Financeiro (Opção 1):**
- vencido, venceu, renegociar, débito

**Casas Jardins (Opção 2):**
- casas jardins, casa jardim, meu empreendimento, minha casa, meu imóvel, entrega, iptu, vistoria, visita

**Relacionamento (Opção 3):**
- relacionamento, atendimento, cliente, dúvida, informação, reclamação, sugestão, extrato, segunda via, contrato, documentação, boleto a vencer, atendente, humano, pessoa, operador

**Comercial (Opção 4):**
- comprar, compra, vendas, venda, comercial, corretor, stand, stande, adquirir, interesse, lançamento

**Assistência Técnica (Opção 5):**
- assistência, chamado, agendamento, problema, defeito, manutenção, reparo, conserto, vazamento, infiltração, pós-entrega, elétrica

**Reconhecimento de números por voz (quando no menu DTMF):**
- "um", "número um", "opção um" → Opção 1
- "dois", "número dois", "opção dois" → Opção 2
- E assim por diante...

## Estrutura do Projeto
ConversationRelayBR/
├── Controllers/
│   └── IncomingCallHTTP.cs                  # Webhook e transferências
│
├── Services/
│   ├── WebSocketService.cs                  # Gerencia comunicação WebSocket
│   └── ConversationService.cs               # Lógica de conversa e estados
│
├── Models/
│   ├── Enums/
│   │   ├── CallFlowState.cs                 # Estados do fluxo (6 estados)
│   │   └── IvrOptions.cs                    # Opções do menu IVR
│   │
│   └── WebSocket/
│       ├── Incoming/                        # SetupMessage, PromptMessage, DtmfMessage
│       ├── Outgoing/                        # TextMessage, EndMessage
│       └── TwilioSettings/                  # Configurações Twilio
│
├── Filters/
│   └── ValidateTwilioRequestAttribute.cs    # Autenticação de webhooks
│
├── ConversationRelayBR.Test/
│   └── ConversationRelayServiceTests.cs     # Testes unitários
│
└── Program.cs                               # Configuração WebSocket e DI


## Endpoints

- **POST /** - Webhook para chamadas Twilio
- **WS /websocket** - Conexão WebSocket para mensagens

## Licença
Paulo Eduardo Furtado Lopes

## Segurança

- Verificação de assinatura `X-Twilio-Signature`
- Pode ser habilitada/desabilitada via `RequestValidationEnabled`
- Usa `RequestValidator` oficial da Twilio SDK

## Testes

(xUnit + Moq)
- Análise de intenções (todas as opções do IVR)
- Validação de entrada DTMF
- Reconhecimento de palavras-chave
- Reconhecimento de números por voz no menu
- Tratamento de entrada não reconhecida

### Execute os Testes
dotnet test

## Roadmap

### Em Desenvolvimento
- Implementar logs detalhados estruturados (ILogger)
- Melhorar tratamento de erros com retry strategies
- Documentar API (Swagger/OpenAPI)

### Planejado - Deploy
- Deploy em servidor de produção
- Configurar CI/CD (GitHub Actions / Azure DevOps)
- Monitoramento de chamadas em tempo real

### Futuras Otimizações
- **Melhorias de UX**:
  - Menu IVR mais dinâmico baseado em feedback do cliente
  - Mensagens personalizadas por horário/contexto
  
- **Integrações**:
  - CRM para validação de dados do cliente
  - Automações (envio de boleto, extrato, agendamentos)
  - Sistema de tickets para assistência técnica
  
- **Analytics & IA**:
  - Análise de sentimentos na fala do cliente
  - Dashboard de métricas de atendimento
  - Machine Learning para melhorar reconhecimento de intenções
  
- **Recursos Avançados**:
  - Suporte a múltiplos idiomas
  - Gravação e transcrição de chamadas
  - Autenticação por voz para segurança
  - Campanhas de marketing via voz
  - Fallback automático para chamadas perdidas
  - Painel administrativo para gestão

## Contribuindo

Projeto em desenvolvimento ativo. Para sugestões ou problemas, abra uma issue no repositório.

## Status do Projeto

🟢 **MVP Funcional** - Sistema pronto para testes em produção
- ✅ Reconhecimento de voz e DTMF
- ✅ Análise de intenções
- ✅ Transferência de chamadas
- ✅ Validação de segurança Twilio
- ✅ Testes unitários básicos

