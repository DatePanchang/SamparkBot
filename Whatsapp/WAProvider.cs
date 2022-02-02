﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

using Microsoft.AspNetCore.Mvc;

using SamparkBot.GupshupMedels;

namespace SamparkBot;
  public class WAProvider
  {
    public WAProvider()
    {
    }

    //private static readonly ConnectionMultiplexer redis;
    static WAProvider() {
      //redis = ConnectionMultiplexer.Connect(
      //      new ConfigurationOptions {
      //        EndPoints = { "host.docker.internal:6379" }
      //      });
    }

    public async Task<ActionResult> ProcessMessage(PostShape request) {
      using HttpClient client = new HttpClient();

      try {
        await SendChatwwotTextMsg();
      } catch (Exception) {
        return Ok();
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