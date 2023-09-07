// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

namespace SamparkBot;
public class WAProvider {
  public WAProvider() {
  }

  //private static readonly ConnectionMultiplexer redis;
  static WAProvider() {
    //redis = ConnectionMultiplexer.Connect(
    //      new ConfigurationOptions {
    //        EndPoints = { "host.docker.internal:6379" }
    //      });
  }

  public static async Task OnReceiveMessage(GupshupModels.IncomingMessage request) {
    try {
      Console.WriteLine($"sender: {request.Payload?.Sender?.Name}");
      await Helper.SendChatwootMsg(request);
    } catch (Exception ex) {
      Console.Error.WriteLine(ex);
      return;
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
