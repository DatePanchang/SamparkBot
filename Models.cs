namespace SamparkBot.Models {
  public record Contact {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;    
  }

  public enum Channel {
    Unknown,
    Whatsapp,
    Facebook,
    Instagram,
    Twitter,
    Telegram
  }
}
