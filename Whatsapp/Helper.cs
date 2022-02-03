using Newtonsoft.Json.Linq;

using SamparkBot.Models;

using System.Net.Http.Headers;

namespace SamparkBot {
  public class Helper {
    private static readonly string providerApiKey;
    private static readonly string gupshupApp;
    private static readonly string chatwootApiKey;
    private static readonly string chatwootInboxId;
    
    private static readonly string whatsAppBizNumber = "919075025309";
    private static readonly string chatwootBaseUrl = "https://www.chatwoot.com/developers/api/#operation/";
    static Helper() {
      //redis = ConnectionMultiplexer.Connect(
      //      new ConfigurationOptions {
      //        EndPoints = { "host.docker.internal:6379" }
      //      });
      providerApiKey = Environment.GetEnvironmentVariable("WA_API_KEY") ?? "";
      gupshupApp = Environment.GetEnvironmentVariable("GUPSHUP_APP") ?? "";
      chatwootApiKey = Environment.GetEnvironmentVariable("CHATWOOT_API_KEY") ?? "";
      chatwootInboxId = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
        ? ""
        : Environment.GetEnvironmentVariable("CHATWOOT_INBOX_DEV_ID") ?? "";
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
      using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{chatwootBaseUrl}contactCreate");
      request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
      request.Headers.TryAddWithoutValidation("api_access_token", chatwootApiKey);

      
      //request.Content = new JsonContent() {

      //};
      request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

      var response = await client.SendAsync(request);
    }
  }
}
