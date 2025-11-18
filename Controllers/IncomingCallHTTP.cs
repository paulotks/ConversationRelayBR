using ConversartionRelayBR.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace ConversartionRelayBR.Controllers
{
    [Route("/")]
    public class IncomingCallHTTP : Controller
    {
        [HttpPost]
        public IActionResult IncomingCallTwilioHTTP()
        {
            var response = new VoiceResponse();
            var connect = new Connect(
                action: new Uri($"{Request.Scheme}://{Request.Host}/transfer")
                );

            var conversationRelay = new ConversationRelay(
                url: "wss://telma-unswitched-walton.ngrok-free.dev/websocket",
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
                var url = GetQueueUrl((IvrOption)option);

                response.Redirect(new Uri(url), method: Twilio.Http.HttpMethod.Post);
            }
            else
            {
                response.Hangup();
            }

            return Content(response.ToString(), "application/xml");
        }

        private static string GetQueueUrl(IvrOption option)
        {
            return option switch
            {
                IvrOption.BoletosVencidos => "https://fgr.vize.solutions/socket/SoftphoneCall/EnqueueWithAction?Fila=64015fe7-baf8-ed11-8f6e-000d3a88fa9d",
                IvrOption.ClienteCasasJardins => "https://fgr.vize.solutions/socket/SoftphoneCall/EnqueueWithAction?Fila=6d71c2a2-baf8-ed11-8f6e-000d3a88fa9d",
                IvrOption.RelacionamentoCliente => "https://fgr.vize.solutions/socket/SoftphoneCall/EnqueueWithAction?Fila=d6ffad19-baf8-ed11-8f6e-000d3a88fa9d",
                IvrOption.StandeVendas => "https://fgr.vize.solutions/socket/SoftphoneCall/EnqueueWithAction?Fila=XXXXXX", // ← DEFINA A FILA
                IvrOption.AssistenciaTecnica => "https://fgr.vize.solutions/socket/SoftphoneCall/EnqueueWithAction?Fila=08f6ce0d-bbf8-ed11-8f6e-000d3a88fa9d",
                _ => "https://fgr.vize.solutions/socket/SoftphoneCall/EnqueueWithAction?Fila=08f6ce0d-bbf8-ed11-8f6e-000d3a88fa9d"
            };
        }
    }
}
