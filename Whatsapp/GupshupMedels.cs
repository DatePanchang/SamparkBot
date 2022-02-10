namespace SamparkBot.GupshupModels {
  public record Payload {
    public string? Text { get; set; }
    public string? Caption { get; set; }
    public string? Url { get; set; }
    public string? ContentType { get; set; }
    public string? Type { get; set; }
    public long? UrlExpiry { get; set; }
  }
  public record ParentPayload {
    public string? Id { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public Payload? Payload { get; set; }
    public Sender? Sender { get; set; }
    public Context? Context { get; set; }
  }

  public record Sender {
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public string? Country_code { get; set; }
    public string? Dial_code { get; set; }
  }

  public record Context {
    public string? Id { get; set; }
    public string? GsId { get; set; }
  }

  public record IncomingMessage {
    public string? App { get; set; }
    public long Timestamp { get; set; }
    public int Version { get; set; }
    public string? Type { get; set; }
    public ParentPayload? Payload { get; set; }
  }
}
