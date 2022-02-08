// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

using Microsoft.AspNetCore.Mvc;

using SamparkBot.GupshupModels;

namespace SamparkBot;
public class WAAggregator {
  public WAAggregator() {
  }

  public static async Task OnReceiveMessage(ChatwootModels.OutgoingMessage message) {
    try {
      await Helper.SendGupshupTextMsg(message);
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
    }
  }
}
