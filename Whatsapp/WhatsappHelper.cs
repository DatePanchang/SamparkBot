using Newtonsoft.Json.Linq;

using System.Net.Http.Headers;

namespace SamparkBot {
  public class WhatsappHelper {
    private static readonly string providerApiKey;
    private static readonly string gupshupApp;
    private static readonly string whatsAppBizNumber = "919075025309";
    static WhatsappHelper() {
      //redis = ConnectionMultiplexer.Connect(
      //      new ConfigurationOptions {
      //        EndPoints = { "host.docker.internal:6379" }
      //      });
      providerApiKey = Environment.GetEnvironmentVariable("WA_API_KEY") ?? "";
      gupshupApp = Environment.GetEnvironmentVariable("GUPSHUP_APP") ?? "";
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

    public static async Task SendChatwwotTextMsg(JObject requestJson, string txtMsg) {
      using var client = new HttpClient();
      using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.gupshup.io/sm/api/v1/msg");
      request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
      request.Headers.TryAddWithoutValidation("apikey", providerApiKey);

      var contentList = new List<string> {
        $"id={Uri.EscapeDataString($"{requestJson.SelectToken("payload.id")}")}",
        $"channel={Uri.EscapeDataString("whatsapp")}",
        $"source={Uri.EscapeDataString("919075025309")}",
        $"destination={Uri.EscapeDataString($"{requestJson.SelectToken("payload.sender.phone")}")}",
        $"src.name={Uri.EscapeDataString($"{requestJson.SelectToken("app")}")}",
        $"message={Uri.EscapeDataString("{\"type\": \"text\",\"text\": \"" + txtMsg + "\"}")}",
        $"disablePreview={Uri.EscapeDataString("true")}"
      };
      request.Content = new StringContent(string.Join("&", contentList));
      request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

      var response = await client.SendAsync(request);
    }
  }
}
