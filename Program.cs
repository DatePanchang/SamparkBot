using SamparkBot;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

// end point for gupshup like provider
app.MapPost("/api/provider", async (SamparkBot.GupshupModels.IncomingMessage message) => {
  Console.WriteLine($"/api/provider: Number {message.Payload?.Sender?.Phone} : {message.Payload?.Payload?.Text}");
  await Helper.SendChatwootMsg(message);
  return Results.Ok();
});

// end point for chatwoot like provider
app.MapPost("/api/aggregator", async (SamparkBot.ChatwootModels.OutgoingMessage message) => {
    Console.WriteLine(message.Content);
    await Helper.SendGupshupTextMsg(message)
});

app.Run();
