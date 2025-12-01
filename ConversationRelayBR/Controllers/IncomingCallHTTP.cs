using ConversartionRelayBR.Filters;
using ConversartionRelayBR.Models.Enums;
using ConversartionRelayBR.Models.WebSocket.TwilioSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace ConversartionRelayBR.Controllers
{
    [Route("/")]
    [ValidateTwilioRequest]
    public class IncomingCallHTTP : Controller
    {
        private readonly TwilioSettings _twilioSettings;

        public IncomingCallHTTP(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
        }

        [HttpPost]
        public IActionResult IncomingCallTwilioHTTP()
        {
            var response = new VoiceResponse();
            var connect = new Connect(
                action: new Uri($"{Request.Scheme}://{Request.Host}/transfer"),
                method: Twilio.Http.HttpMethod.Post
                );

            var conversationRelay = new ConversationRelay(
                url: _twilioSettings.WebSocketUrl,
                welcomeGreeting: "Obrigado por entrar em contato com a FGR Incorporações.Em poucas palavras, me diga o motivo do seu contato… Ou aguarde para ouvir as opções.",
                language:"pt-BR",
                ttsProvider:"ElevenLabs",
                voice: "m151rjrbWXbBqyq56tly-1.1_0.8_0.6",
                dtmfDetection: true,
                hints: "FGR,boletos,vencidos,Jardins,relacionamento,cliente,extratos,pagamentos,vendas,corretor,stande,assistência,técnica,condomínios,horizontais,incorporações"
                );

            //conversationRelay.Hints = "FGR,atendimento,conta corrente,poupança,cartão,empréstimo,financiamento,PIX,transferência,saldo,extrato,segunda via,cancelamento,bloqueio,desbloqueio,por favor,obrigada,com licença,desculpa,problema,dúvida,informação";

            connect.Append(conversationRelay);
            response.Append(connect);

            Console.WriteLine(response.ToString());

            return Content(response.ToString(), "application/xml");
        }

        [HttpPost("transfer")]
        public IActionResult Transfer([FromForm] string? HandoffData)
        {
            var response = new VoiceResponse();

            if (!string.IsNullOrEmpty(HandoffData) &&
                int.TryParse(HandoffData, out int option) &&
                Enum.IsDefined(typeof(IvrOption), option))
            {
                var ivrOption = (IvrOption)option;

                if (ivrOption == IvrOption.StandVendas)
                {
                    response.Dial(_twilioSettings.StandPhoneNumber);
                }
                else
                {
                    var url = GetQueueUrl(ivrOption);

                    response.Redirect(
                        method: Twilio.Http.HttpMethod.Post,
                        url: new Uri(url)
                    );
                }
            }
            else
            {
                response.Hangup();
            }

            return Content(response.ToString(), "application/xml");
        }

        [HttpGet("transfer")]
        public IActionResult TransferGet([FromQuery] string? HandoffData)
        {
            var response = new VoiceResponse();

            if (!string.IsNullOrEmpty(HandoffData) &&
                int.TryParse(HandoffData, out int option) &&
                Enum.IsDefined(typeof(IvrOption), option))
            {
                var ivrOption = (IvrOption)option;

                if (ivrOption == IvrOption.StandVendas)
                {
                    response.Dial(_twilioSettings.StandPhoneNumber);
                } else
                {
                    var url = GetQueueUrl(ivrOption);

                    response.Redirect(
                        method: Twilio.Http.HttpMethod.Post,
                        url: new Uri(url)
                    );
                }
            }
            else
            {
                response.Hangup();
            }

            Console.WriteLine(response.ToString());

            return Content(response.ToString(), "application/xml");
        }

        private string GetQueueUrl(IvrOption option)
        {
            return option switch
            {
                IvrOption.BoletosVencidos => $"{_twilioSettings.SoftPhoneUrl}",
                IvrOption.ClienteCasasJardins => $"{_twilioSettings.SoftPhoneUrl}",
                IvrOption.RelacionamentoCliente => $"{_twilioSettings.SoftPhoneUrl}",
                IvrOption.AssistenciaTecnica => $"{_twilioSettings.SoftPhoneUrl}",
                _ => $"{_twilioSettings.SoftPhoneUrl}"
            };
        }
    }
}
