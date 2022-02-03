using SamparkBot.ChatwootMedels;

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

app.MapPost("/api/wa", async (OutgoingMessage message) => {
  Console.WriteLine(message.Content);
  // await Helper.SendGupshupTextMsg()
});

app.Run();
