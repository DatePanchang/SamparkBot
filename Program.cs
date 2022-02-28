using Microsoft.AspNetCore.Http.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SamparkBot;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config => {
  //some swagger configuration code.

  //use fully qualified object names
  config.CustomSchemaIds(x => x.FullName);
});

JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
  ContractResolver = new DefaultContractResolver {
    NamingStrategy = new SnakeCaseNamingStrategy()
  }
};

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseMiddleware<ApiKeyMiddleware>();

// end point for gupshup like provider
app.MapPost("/api/wa/provider", async (SamparkBot.GupshupModels.IncomingMessage message) => {
  Console.WriteLine($"/api/wa/provider: Number {message.Payload?.Sender?.Phone} : {message.Payload?.Payload?.Text}");
  await WAProvider.OnReceiveMessage(message);
  return Results.Ok();
});

// end point for chatwoot like provider
app.MapPost("/api/wa/aggregator", async (SamparkBot.ChatwootModels.OutgoingMessage message) => {
  Console.WriteLine(message.Content);
  await WAAggregator.OnReceiveMessage(message);
});

app.Run();
