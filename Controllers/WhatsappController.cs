﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace sampark_whatsapp_bot.Controllers {
  // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
  // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
  // achieved by specifying a more specific type for the bot constructor argument.
  [Route("api/wa")]
  [ApiController]
  public class WABotController : ControllerBase
  {
    public WABotController()
    {
    }

    private static readonly string gupshupApiKey;
    //private static readonly ConnectionMultiplexer redis;
    static WABotController() {
      //redis = ConnectionMultiplexer.Connect(
      //      new ConfigurationOptions {
      //        EndPoints = { "host.docker.internal:6379" }
      //      });
      gupshupApiKey = Environment.GetEnvironmentVariable("WA_API_KEY");
    }

    [HttpPost, HttpGet]
    public async Task<ActionResult> PostAsync()
    {

      var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
      var requestJson = JObject.Parse(requestBody);

      using HttpClient client = new HttpClient();

      static async Task SendTextMsg(JObject requestJson, HttpClient client, string txtMsg)
      {
        using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.gupshup.io/sm/api/v1/msg"))
        {
          request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
          request.Headers.TryAddWithoutValidation("apikey", gupshupApiKey);

          var contentList = new List<string>
          {
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

      try {
        var recentUser = requestJson["payload"]?["context"] != null;  //await IsRecentUser($"{requestJson.SelectToken("payload.sender.phone")}");
        if (recentUser) {
          await SendTextMsg(requestJson, client, "```आमच्या विविध उपक्रमांची, सेवांची माहिती आधी आपल्याला पाठवली आहे.\t\nया व्यतिरिक्त कुठल्या कारणासाठी आमच्याशी संपर्क साधावयाचा असल्यास पुढील लिंकवर संपर्क करावा. धन्यवाद !```\t\n\t\n*Customer Support*\t\n https://t.me/panchangsupportbot");
        } else {
          await SendTextMsg(requestJson, client, "```दाते पंचांगाच्या WhatsApp सेवेमध्ये आपले स्वागत आहे.\t\nसोबत दिलेल्या लिंक्सवर क्लिक करून या सेवांचा लाभ घेऊ शकता.```\t\n\t\n*वेबसाइट*\t\n https://datepanchang.com \t\n\t\n*सेवा*\t\n*जन्मपत्रिका | कुंडली*\t\n https://web.datepanchang.com/#/Seva/Patrikaa \t\n*विवाह मेलन*\t\n https://web.datepanchang.com/#/Seva/Melan \t\n*विविध मुहूर्त*\t\n https://web.datepanchang.com/ \t\n\t\n*Social Handles*\t\n*Facebook*\t\n https://www.facebook.com/DatePanchang \t\n*Youtube*\t\n https://www.youtube.com/channel/UCMFEM4bpAiYDdbSrFKuq6RA \t\n*Instagram*\t\n https://www.instagram.com/date.panchang/ \t\n*Twitter*\t\n https://twitter.com/DatePanchang \t\n*Telegram*\t\n https://t.me/datepanchang \t\n\t\n*Customer Support*\t\n https://t.me/panchangsupportbot");
        }
      } catch (Exception) {
        await SendTextMsg(requestJson, client, "```दाते पंचांगाच्या WhatsApp सेवेमध्ये आपले स्वागत आहे.\t\nसोबत दिलेल्या लिंक्सवर क्लिक करून या सेवांचा लाभ घेऊ शकता.```\t\n\t\n*वेबसाइट*\t\n https://datepanchang.com \t\n\t\n*सेवा*\t\n*जन्मपत्रिका | कुंडली*\t\n https://web.datepanchang.com/#/Seva/Patrikaa \t\n*विवाह मेलन*\t\n https://web.datepanchang.com/#/Seva/Melan \t\n*विविध मुहूर्त*\t\n https://web.datepanchang.com/ \t\n\t\n*Social Handles*\t\n*Facebook*\t\n https://www.facebook.com/DatePanchang \t\n*Youtube*\t\n https://www.youtube.com/channel/UCMFEM4bpAiYDdbSrFKuq6RA \t\n*Instagram*\t\n https://www.instagram.com/date.panchang/ \t\n*Twitter*\t\n https://twitter.com/DatePanchang \t\n*Telegram*\t\n https://t.me/datepanchang \t\n\t\n*Customer Support*\t\n https://t.me/panchangsupportbot");
      }
      return Ok();
    }

    //private async Task<bool> IsRecentUser(string mobile) {
    //  try {
    //    var db = redis.GetDatabase();
    //    if (await db.KeyExistsAsync(mobile)) {
    //      return true;
    //    } else {
    //      await db.StringSetAsync(mobile, "", TimeSpan.FromDays(3));
    //      return false;
    //    }
    //  } catch (Exception ex) {
    //    return false;
    //  }
    //}
  }
}