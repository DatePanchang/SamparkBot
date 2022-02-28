namespace SamparkBot.ChatwootModels {
  public class ContactSearch {
    public List<Contact> Payload = new();
  }

  public class ContactPayload {
    public Contact Payload = new();
  }

  public class CreateContactResponse {
    public CreateContactPayload Payload = new ();
  }
  public class CreateContactPayload {
    public Contact Contact { get; set; } = new ();
    public ContactInbox ContactInbox { get; set; } = new ();
  }

  public class ContactConversaionsPayload {
    public List<Conversation> Payload { get; set; } = new();
  }

  public class ContactInbox {
    public Inbox Inbox { get; set; } = new ();
    public string SourceId { get; set; } = string.Empty;
  }

  public class Contact {
    public string Email { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public string PhoneNumber { get; set; } = String.Empty;
    public string Thumbnail { get; set; } = String.Empty;
    public Dictionary<string, string>? AdditionalAttributes { get; set; }
    public List<ContactInbox> ContactInboxes { get; set; } = new();
    public int Id { get; set; }
    public string AvailabilityStatus { get; set; } = String.Empty;
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
    public int Id { get; set; }
    public int InboxId { get; set; }
    public string Status { get; set; } = String.Empty;
    public int AgentLastSeenAt { get; set; }
    public int ContactLastSeenAt { get; set; }
    public int Timestamp { get; set; }
    public Dictionary<string, string>? AdditionalAttributes { get; set; }
  }

  /// <summary>
  /// Outgoing from chatwoot. Chatwoot agent sends message to customer
  /// </summary>
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
    public string Event { get; set; } = String.Empty;
  }
}
