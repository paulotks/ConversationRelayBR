using ConversartionRelayBR.Models.WebSocket.TwilioSettings;
using ConversartionRelayBR.Services;
using Microsoft.Extensions.Options;
using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddScoped<ConversationService>();

builder.Services.Configure<TwilioSettings>(
    builder.Configuration.GetSection("TwilioSettings")
);


var app = builder.Build();

var twilioSettings = app.Services.GetRequiredService<IOptions<TwilioSettings>>().Value;



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseWebSockets();

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.Map("/websocket", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        Console.WriteLine("Nova conexão WebSocket");

        var webSocketService = app.Services.GetRequiredService<WebSocketService>();

        using var websocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketConnection(websocket, webSocketService);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();

static async Task HandleWebSocketConnection(System.Net.WebSockets.WebSocket webSocket, WebSocketService webScocketService)
{
    var conversationService = new ConversationService(webScocketService);
    var buffer = new byte[1024 * 4];

    Console.WriteLine("Aguardando Mensagens da Twilio...");

    while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
        {
            var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
            //Console.WriteLine($"RecebidoTwilio: {message}");

            await conversationService.ProcessMessageAsync(webSocket, message);

        }
        else if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
        {
            await webSocket.CloseAsync(
                System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                "",
                CancellationToken.None
            );
        }
    }
}