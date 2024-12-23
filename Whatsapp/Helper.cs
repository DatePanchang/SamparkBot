using SamparkBot.GupshupModels;

using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace SamparkBot {
  public class Helper {
    private const string gupshupUrl = "https://api.gupshup.io/sm/api/v1/msg";
    private const string contactIdKey = "contactId";

    private static readonly string whatsAppBizNumber;
    private static readonly string providerApiKey;
    private static readonly string gupshupApp;
    private static readonly string aggregatorApiKey;
    private static readonly int chatwootInboxId;
    private static readonly string aggregatorBaseUrl;

    static Helper() {
      whatsAppBizNumber = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
        ? "919075025309"
        : "917834811114";
      
      providerApiKey = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
        ? Environment.GetEnvironmentVariable("WA_API_PROD_KEY") ?? throw new Exception("Ebvironment variable WA_API_PROD_KEY not found")
        : Environment.GetEnvironmentVariable("WA_API_DEV_KEY") ?? throw new Exception("Ebvironment variable WA_API_DEV_KEY not found");

      gupshupApp = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
        ? Environment.GetEnvironmentVariable("GUPSHUP_PROD_APP") ?? throw new Exception("Ebvironment variable GUPSHUP_PROD_APP not found")
        : Environment.GetEnvironmentVariable("GUPSHUP_DEV_APP") ?? throw new Exception("Ebvironment variable GUPSHUP_DEV_APP not found");

      aggregatorApiKey = Environment.GetEnvironmentVariable("AGGREGATOR_API_KEY") ?? throw new Exception("Ebvironment variable AGGREGATOR_API_KEY not found");
      chatwootInboxId = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
        ? int.Parse(Environment.GetEnvironmentVariable("CHATWOOT_INBOX_PROD_ID") ?? throw new Exception("Ebvironment variable CHATWOOT_INBOX_PROD_ID not found"))
        : int.Parse(Environment.GetEnvironmentVariable("CHATWOOT_INBOX_DEV_ID") ?? throw new Exception("Ebvironment variable CHATWOOT_INBOX_DEV_ID not found"));
      aggregatorBaseUrl = $"{Environment.GetEnvironmentVariable("CHATWOOT_BASE_URL") ?? throw new Exception("Ebvironment variable WA_API_PROD_KEY not found")}/api/v1/accounts/1/";
    }

    internal static async Task SendChatwootMsg(IncomingMessage message) {
      // messages from gupshup not having sender should be ignored
      if (message.Payload?.Sender is null) {
        return;
      }
      var conversation = await GetOrCreateChatwootConversationByPhone(message.Payload.Sender);
      if (message.Payload?.Type == "text") {
        await SendChatwootTextMsg(conversation, message.Payload?.Payload?.Text ?? "");
      }
    }

    private static async Task<ChatwootModels.Conversation> GetOrCreateChatwootConversationByContact(ChatwootModels.Contact chatwootContact) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{aggregatorBaseUrl}contacts/{chatwootContact.Id}/conversations");
      AddChatwootHeaders(request);

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        string responseString = await response.Content.ReadAsStringAsync();
        var contactConversationsPayload = JsonConvert.DeserializeObject<ChatwootModels.ContactConversaionsPayload>(responseString);
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
        var responseString = await response.Content.ReadAsStringAsync();
        var contactInbox = JsonConvert.DeserializeObject<ChatwootModels.ContactInbox>(responseString);
        if (contactInbox is null) {
          throw new Exception($"Scope: CreateContactInbox, Message: Deserialization returned null contact inbox in create contactInbox for contact");
        }
        return contactInbox;
      } else {
        throw new Exception($"Scope: CreateContactInbox, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.Conversation> CreateChatwootConversation(int contactId, string sourceId) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}conversations");
      AddChatwootHeaders(request);

      request.Content = JsonContent.Create(new {
        source_id = sourceId,
        inbox_id = chatwootInboxId,
        contact_id = contactId,
        additional_attributes = new Dictionary<string, string> { { contactIdKey, $"{contactId}"} }
      });

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var conversation = JsonConvert.DeserializeObject<ChatwootModels.Conversation>(await response.Content.ReadAsStringAsync());
        if (conversation == null) {
          throw new Exception($"Scope: CreateChatwootConversation, Message: Deserialization returned null conversation in create conversation");
        }
        return conversation;
      } else {
        throw new Exception($"Scope: CreateChatwootConversation, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.Conversation> GetOrCreateChatwootConversationByPhone(Sender sender) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{aggregatorBaseUrl}api/v1/accounts/1/contacts/search?q={sender.Phone?.Replace("+", "")}");
      AddChatwootHeaders(request);

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        ChatwootModels.Contact contact;
        ChatwootModels.Conversation conversation;
        var responseString = await response.Content.ReadAsStringAsync();
        var contactSearch = JsonConvert.DeserializeObject<ChatwootModels.ContactSearch>(responseString);
        if (contactSearch == null) {
          throw new Exception($"Scope: GetChatwootConversationByNumber, Message: Deserialization returned null");
        } else if (contactSearch.Payload.Count == 0) {
          var contactPayload = await CreateChatwootContact(sender);
          contact = contactPayload.Contact;
          conversation = await CreateChatwootConversation(contactPayload.Contact.Id, contactPayload.ContactInbox.SourceId);
        } else {
          contact = contactSearch.Payload[0];
          conversation = await GetOrCreateChatwootConversationByContact(contact);
        }
        return conversation;
      } else {
        throw new Exception($"Scope: GetChatwootConversationByNumber, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.CreateContactPayload> CreateChatwootContact(Sender sender) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}contacts");
      AddChatwootHeaders(request);

      if (sender.Phone is null || sender.Phone.Length <= 10) {
        throw new Exception($"Scope: CreateChatwootContact, Message: Sender phone '{sender.Phone}' is not valid");
      }

      request.Content = JsonContent.Create(new {
        inbox_id = chatwootInboxId,
        name = sender.Name,
        phone_number = sender.Phone.StartsWith("+")
          ? sender.Phone 
          : "+" + sender.Phone
      });

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var createContactResponse = JsonConvert.DeserializeObject<ChatwootModels.CreateContactResponse>(await response.Content.ReadAsStringAsync());
        if (createContactResponse == null) {
          throw new Exception($"Scope: CreateChatwootContact, Message: Deserialization of create contact response of Chatwoot returned null");
        } else {
          return createContactResponse.Payload;
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
      if (message.Sender?.Type == "contact") {
        return;
      }

      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), gupshupUrl);
      request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
      request.Headers.TryAddWithoutValidation("apikey", providerApiKey);

      int? conversationId = message.Conversation?.Id;
      if (conversationId is null) {
        throw new Exception($"Scope: SendGupshupTextMsg, Message: Conversation id not valid in webhook payload from chatwood");
      }
      var conversation = await GetChatwootConversationById(conversationId.Value);

      if (conversation.AdditionalAttributes is null || !conversation.AdditionalAttributes.ContainsKey(contactIdKey)) {
        throw new Exception($"Scope: SendGupshupTextMsg, Message: Conversation doesnot contain additional attributes or an attribute with contactId key");
      }

      var contact = await GetChatwootContactById(int.Parse(conversation.AdditionalAttributes[contactIdKey]));

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

      if (response.StatusCode != System.Net.HttpStatusCode.OK) {
        throw new Exception($"Scope: SendGupshupTextMsg, Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }

    }

    private static async Task<ChatwootModels.Conversation> GetChatwootConversationById(int id) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{aggregatorBaseUrl}conversations/{id}");
      AddChatwootHeaders(request);

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var responseString = await response.Content.ReadAsStringAsync();
        var conversation = JsonConvert.DeserializeObject<ChatwootModels.Conversation>(responseString);
        if (conversation is null) {
          throw new Exception($"Scope: GetChatwootConversationById, Message: Deserialization returned null conversation");
        }
        return conversation;
      } else {
        throw new Exception($"Scope: GetChatwootConversationById ({id}), Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    private static async Task<ChatwootModels.Contact> GetChatwootContactById(int id) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{aggregatorBaseUrl}contacts/{id}");

      AddChatwootHeaders(request);
      var response = await client.SendAsync(request);
      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        string responseString = await response.Content.ReadAsStringAsync();
        var contactPayload = JsonConvert.DeserializeObject<ChatwootModels.ContactPayload>(responseString);
        if (contactPayload is null) {
          throw new Exception($"Scope: GetChatwootContactById, Message: Deserialized contactPayload is null");
        }
        return contactPayload.Payload;
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
