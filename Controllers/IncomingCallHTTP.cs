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
                welcomeGreeting: "Bem vindo ao canal de atendimento da FGR, Este é um canal em desenvolvimento. Obrigado por ligar",
                language: "pt-BR"
                );

            //conversationRelay.Hints = "FGR,atendimento,conta corrente,poupança,cartão,empréstimo,financiamento,PIX,transferência,saldo,extrato,segunda via,cancelamento,bloqueio,desbloqueio,por favor,obrigada,com licença,desculpa,problema,dúvida,informação";


            connect.Append(conversationRelay);
            response.Append(connect);

            Console.WriteLine(response.ToString());

            return Content(response.ToString(), "application/xml");
        }
    }
}
