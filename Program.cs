using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

var accountSid = app.Configuration["TWILIO_ACCOUNT_SID"];
var authToken = app.Configuration["TWILIO_AUTH_TOKEN"];

// Validate configuration
if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
{
    throw new InvalidOperationException("Twilio credentials not found in configuration. Please check your appsettings.json file.");
}

TwilioClient.Init(accountSid, authToken);

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
        Console.WriteLine("Chegou no WebSocket");
        using var websocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketConnection(websocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();

static async Task HandleWebSocketConnection(System.Net.WebSockets.WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];

    while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
        {
            var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"RecebidoTwilio: {message}");

            //var response = "Echo: " + message;
            //var responseBytes = System.Text.Encoding.UTF8.GetBytes(response);

            //await webSocket.SendAsync(
            //    new ArraySegment<byte>(responseBytes),
            //    System.Net.WebSockets.WebSocketMessageType.Text,
            //    true,
            //    CancellationToken.None
            //    );
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