namespace SamparkBot.ChatwootMedels {
  public record Contact {
    public string InboxId { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string PhoneNumber { get; set; } = String.Empty;
    public string Identifier { get; set; } = String.Empty;
  }

  public record Sender {
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Avatar { get; set; } = String.Empty;
    public string Type { get; set; } = String.Empty;
  }

  public record Inbox {
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
  }

  public record Conversation {
    public string Channel { get; set; } = String.Empty;
    public int Id { get; set; }
    public int InboxId { get; set; }
    public string Status { get; set; } = String.Empty;
    public int AgentLastSeenAt { get; set; }
    public int ContactLastSeenAt { get; set; }
    public int Timestamp { get; set; }
  }

  public record Account {
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
  }

  public record OutgoingMessage {
    public int Id { get; set; }
    public string Content { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public string MessageType { get; set; } = String.Empty;
    public string ContentType { get; set; } = String.Empty;
    public string SourceId { get; set; } = String.Empty;
    public Sender? Sender { get; set; }
    public Inbox? Inbox { get; set; }
    public Conversation? Conversation { get; set; }
    public Account? Account { get; set; }
    public string Event { get; set; } = String.Empty;
  }
}
