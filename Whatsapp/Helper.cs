using Newtonsoft.Json.Linq;

using SamparkBot.GupshupModels;
using SamparkBot.Models;

using System.Net.Http.Headers;
using System.Text.Json;

namespace SamparkBot {
  public class Helper {
    private static readonly string providerApiKey;
    private static readonly string gupshupApp;
    private static readonly string aggregatorApiKey;
    private static readonly string chatwootInboxId;
    
    private static readonly string whatsAppBizNumber = "919075025309";
    private static readonly string aggregatorBaseUrl = "https://crm-portal-web.crm.datepanchang.com/api/v1/accounts/2/";
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
    }

    internal static async Task SendChatwootMsg(IncomingMessage message) {
      var chatwootContact = await GetChatwootContactByNumber(message.Payload?.Sender?.Phone);
      if (message.Payload?.Type == "text") {
        await SendChatwootTextMsg();
      }
    }

    private static async Task<ChatwootModels.Contact> GetChatwootContactByNumber(string? phone) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{aggregatorBaseUrl}contacts/search?q={phone?.Replace("+", "")}");
      request.Headers.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
      request.Headers.TryAddWithoutValidation("api_access_token", aggregatorApiKey);

      var response = await client.SendAsync(request);

      if (response.StatusCode == System.Net.HttpStatusCode.OK) {
        var contact = JsonSerializer.Deserialize<ChatwootModels.Contact>(await response.Content.ReadAsStringAsync());
      } else {
        throw new Exception($"Status: {response.StatusCode}, Message: {await response.Content.ReadAsStringAsync()}");
      }
    }

    public static async Task SendGupshupTextMsg(string id, string phone, string txtMsg) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.gupshup.io/sm/api/v1/msg");
      request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
      request.Headers.TryAddWithoutValidation("apikey", providerApiKey);

      var contentList = new List<string> {
        $"id={Uri.EscapeDataString(id)}",
        $"channel={Uri.EscapeDataString("whatsapp")}",
        $"source={Uri.EscapeDataString(whatsAppBizNumber)}",
        $"destination={Uri.EscapeDataString(phone)}",
        $"src.name={Uri.EscapeDataString(gupshupApp)}",
        $"message={Uri.EscapeDataString("{\"type\": \"text\",\"text\": \"" + txtMsg + "\"}")}",
        $"disablePreview={Uri.EscapeDataString("true")}"
      };
      request.Content = new StringContent(string.Join("&", contentList));
      request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

      var response = await client.SendAsync(request);
    }

    public static async Task SendChatwootTextMsg(Contact contact, Channel channel, string txtMsg) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{aggregatorBaseUrl}contactCreate");
      request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
      request.Headers.TryAddWithoutValidation("api_access_token", aggregatorApiKey);

      
      //request.Content = new JsonContent() {

      //};

      var response = await client.SendAsync(request);
    }
  }
}
