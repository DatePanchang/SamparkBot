using SamparkBot.GupshupModels;

using System.Net.Http.Headers;
using System.Text.Json;

namespace SamparkBot {
  public class Helper {
    private const string gupshupUrl = "https://api.gupshup.io/sm/api/v1/msg";
    private const string whatsAppBizNumber = "919075025309";

    private static readonly string providerApiKey;
    private static readonly string gupshupApp;
    private static readonly string aggregatorApiKey;
    private static readonly string chatwootInboxId;
    private static readonly string aggregatorBaseUrl;

    static Helper() {
      //redis = ConnectionMultiplexer.Connect(
      //      new ConfigurationOptions {
      //        EndPoints = { "host.docker.internal:6379" }
      //      });
      providerApiKey = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
        ? Environment.GetEnvironmentVariable("WA_API_PROD_KEY") ?? ""
        : Environment.GetEnvironmentVariable("WA_API_DEV_KEY") ?? "";
      gupshupApp = Environment.GetEnvironmentVariable("GUPSHUP_APP") ?? "";
      aggregatorApiKey = Environment.GetEnvironmentVariable("AGGREGATOR_API_KEY") ?? "";
      chatwootInboxId = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
        ? Environment.GetEnvironmentVariable("CHATWOOT_INBOX_PROD_ID") ?? ""
        : Environment.GetEnvironmentVariable("CHATWOOT_INBOX_DEV_ID") ?? "";
      aggregatorBaseUrl = $"{Environment.GetEnvironmentVariable("CHATWOOT_BASE_URL") ?? ""}/api/v1/accounts/2/";
    }

    internal static async Task SendChatwootMsg(IncomingMessage message) {
      if (message.Payload?.Sender is null) {
        throw new Exception($"Scope: SendChatwootMsg, Message: Sender null in gupshup respose");
      }
      var conversation = await GetChatwootConversationByNumber(message.Payload.Sender);
      if (message.Payload?.Type == "text") {
        await SendChatwootTextMsg(conversation, message.Payload?.Payload?.Text ?? "");
      }
    }

    private static async Task<ChatwootModels.Conversation> GetChatwootConversationByContact(ChatwootModels.Contact chatwootContact) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{aggregatorBaseUrl}contacts/{chatwootContact.Id}/conversations");
      AddChatwootHeaders(request);

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var contactConversationsPayload = JsonSerializer.Deserialize<ChatwootModels.ContactConversaionsPayload>(await response.Content.ReadAsStringAsync());
        if (contactConversationsPayload == null) {
          throw new Exception($"Scope: GetChatwootConversationByContact, Message: Deserialization returned null contact conversations payload");
        }
        var conversation = contactConversationsPayload.Payload
          .OrderByDescending(conversation => conversation.Timestamp)
          .FirstOrDefault(conv => conv.Status != "resolved");

        if (conversation == null) {
          var inbox = chatwootContact.ContactInboxes.FirstOrDefault(cinbox => cinbox.Inbox.Id == chatwootInboxId);
          if (inbox is null) {
            inbox = await CreateContactInbox(chatwootContact);
            if (inbox is null) {
              throw new Exception($"Scope: GetChatwootConversationByContact, Message: Contact does not contain inbox for whatsapp");
            }
          }
          conversation = await CreateChatwootConversation(chatwootContact.Id, inbox.SourceId);
        }
        return conversation;
      } else {
        throw new Exception($"Scope: GetChatwootConversationByContact, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.ContactInbox> CreateContactInbox(ChatwootModels.Contact chatwootContact) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}contacts/{chatwootContact.Id}/contact_inboxes");
      AddChatwootHeaders(request);

      request.Content = JsonContent.Create(new {
        inbox_id = chatwootInboxId
      });

      var response = await client.SendAsync(request);
      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var contactInbox = JsonSerializer.Deserialize<ChatwootModels.ContactInbox>(await response.Content.ReadAsStringAsync());
        if (contactInbox is null) {
          throw new Exception($"Scope: CreateContactInbox, Message: Deserialization returned null contact inbox in create contactInbox for contact");
        }
        return contactInbox;
      } else {
        throw new Exception($"Scope: CreateContactInbox, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.Conversation> CreateChatwootConversation(string contactId, string sourceId) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}conversations");
      AddChatwootHeaders(request);

      request.Content = JsonContent.Create(new {
        source_id = sourceId,
        inbox_id = chatwootInboxId,
        contact_id = contactId,
      });

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var conversation = JsonSerializer.Deserialize<ChatwootModels.Conversation>(await response.Content.ReadAsStringAsync());
        if (conversation == null) {
          throw new Exception($"Scope: CreateChatwootConversation, Message: Deserialization returned null conversation in create conversation");
        }
        return conversation;
      } else {
        throw new Exception($"Scope: CreateChatwootConversation, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.Conversation> GetChatwootConversationByNumber(Sender sender) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{aggregatorBaseUrl}contacts/search?q={sender.Phone?.Replace("+", "")}");
      AddChatwootHeaders(request);

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        ChatwootModels.Contact contact;
        ChatwootModels.Conversation conversation;
        var contactSearch = JsonSerializer.Deserialize<ChatwootModels.ContactSearch>(await response.Content.ReadAsStringAsync());
        if (contactSearch == null) {
          throw new Exception($"Scope: GetChatwootConversationByNumber, Message: Deserialization returned null");
        } else if (contactSearch.Payload.Count == 0) {
          var contactPayload = await CreateChatwootContact(sender);
          contact = contactPayload.Contact;
          conversation = await CreateChatwootConversation(contactPayload.Contact.Id, contactPayload.ContactInbox.SourceId);
        } else {
          contact = contactSearch.Payload[0];
          conversation = await GetChatwootConversationByContact(contact);
        }
        return conversation;
      } else {
        throw new Exception($"Scope: GetChatwootConversationByNumber, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.ContactPayload> CreateChatwootContact(Sender sender) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}contacts");
      AddChatwootHeaders(request);

      request.Content = JsonContent.Create(new {
        inbox_id = chatwootInboxId,
        name = sender.Name,
        phone_number = sender.Phone
      });

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var contactPayload = JsonSerializer.Deserialize<ChatwootModels.ContactPayload>(await response.Content.ReadAsStringAsync());
        if (contactPayload == null) {
          throw new Exception($"Scope: CreateChatwootContact, Message: Deserialization of create contact response of Chatwoot returned null");
        } else {
          return contactPayload;
        }
      } else {
        throw new Exception($"Scope: CreateChatwootContact, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static void AddChatwootHeaders(HttpRequestMessage request) {
      request.Headers.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
      request.Headers.TryAddWithoutValidation("api_access_token", aggregatorApiKey);
    }

    public static async Task SendGupshupTextMsg(ChatwootModels.OutgoingMessage message) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), gupshupUrl);
      request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
      request.Headers.TryAddWithoutValidation("apikey", providerApiKey);

      if (message.Sender?.Id is null) {
        throw new Exception($"Scope: SendGupshupTextMsg, Message: Sender id not valid in webhook payload from chatwood");
      }
      var contact = await GetChatwootContactById(message.Sender.Id);

      var contentList = new List<string> {
        $"channel={Uri.EscapeDataString("whatsapp")}",
        $"source={Uri.EscapeDataString(whatsAppBizNumber)}",
        $"destination={Uri.EscapeDataString(contact.PhoneNumber.Replace("+", ""))}",
        $"src.name={Uri.EscapeDataString(gupshupApp)}",
        $"message={Uri.EscapeDataString("{\"type\": \"text\",\"text\": \"" + message.Content + "\"}")}",
        $"disablePreview={Uri.EscapeDataString("true")}"
      };
      request.Content = new StringContent(string.Join("&", contentList));
      request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

      var response = await client.SendAsync(request);
    }

    private static async Task<ChatwootModels.Contact> GetChatwootContactById(string id) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}contacts/{id}");

      AddChatwootHeaders(request);
      var response = await client.SendAsync(request);
      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var contactPayload = JsonSerializer.Deserialize<ChatwootModels.ContactPayload>(await response.Content.ReadAsStringAsync());
        if (contactPayload is null) {
          throw new Exception($"Scope: GetChatwootContactById, Message: Deserialized contactPayload is null");
        }
        return contactPayload.Contact;
      } else {
        throw new Exception($"Scope: GetChatwootContactById, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    public static async Task SendChatwootTextMsg(ChatwootModels.Conversation conversation, string txtMsg) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}conversations/{conversation.Id}/messages");

      AddChatwootHeaders(request);

      request.Content = JsonContent.Create(new {
        content = txtMsg,
        message_type = "incoming",
        @private = true
      });

      var response = await client.SendAsync(request);

      if (response.StatusCode != System.Net.HttpStatusCode.OK) {
        throw new Exception($"Scope: SendChatwootTextMsg, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }
  }
}
