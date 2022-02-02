// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

using Microsoft.AspNetCore.Mvc;

namespace SamparkBot;
public class WAAggregator {
  public WAAggregator() {
  }

  private static readonly string aggregatorApiKey;
  //private static readonly ConnectionMultiplexer redis;
  static WAAggregator() {
    //redis = ConnectionMultiplexer.Connect(
    //      new ConfigurationOptions {
    //        EndPoints = { "host.docker.internal:6379" }
    //      });
    aggregatorApiKey = Environment.GetEnvironmentVariable("CHATWOOT_API_KEY") ?? "";
  }

  public async Task OnReceiveMessage(string messageId, string phone, string message) {
    try {
      await WhatsappHelper.SendGupshupTextMsg();
    } catch (Exception) {
    }
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