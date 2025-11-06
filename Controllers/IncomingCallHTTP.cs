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
            var connect = new Connect();

            var conversationRelay = new ConversationRelay(
                url: "wss://telma-unswitched-walton.ngrok-free.dev/websocket",
                welcomeGreeting: "Obrigado por entrar em contato com a FGR Incorporações.Em poucas palavras, me diga o motivo do seu contato… Ou aguarde para ouvir as opções.",
                language:"pt-BR",
                ttsProvider:"ElevenLabs",
                voice: "m151rjrbWXbBqyq56tly"
                );

            //conversationRelay.Hints = "FGR,atendimento,conta corrente,poupança,cartão,empréstimo,financiamento,PIX,transferência,saldo,extrato,segunda via,cancelamento,bloqueio,desbloqueio,por favor,obrigada,com licença,desculpa,problema,dúvida,informação";


            connect.Append(conversationRelay);
            response.Append(connect);

            Console.WriteLine(response.ToString());

            return Content(response.ToString(), "application/xml");
        }
    }
}
