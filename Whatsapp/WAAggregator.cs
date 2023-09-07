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
      if (message.Sender?.Type.ToLower() == "user") {
        Console.WriteLine($"from aggregator (chatwoot) sender: {message.Sender}, message: {message.Content}");
        await Helper.SendGupshupTextMsg(message);
        Console.WriteLine($"Message sent to Gupshup: {message.Content}");
      } 
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
    }
  }
}
