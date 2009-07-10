/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using SpikeLite.Domain.Authentication;

namespace SpikeLite.Modules.Puppet
{
    public class PuppetModule : ModuleBase
    {
        public override void HandleRequest(Request request)
        {
            string[] messageArray = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestType == RequestType.Private
                && request.RequestFrom.AccessLevel >= AccessLevel.Root
                && messageArray[0].Equals("~puppet", StringComparison.OrdinalIgnoreCase)
                && messageArray.Length >= 4)
            {
                string target = messageArray[2].Trim();
                string message = string.Empty;

                for (int i = 3; i < messageArray.Length; i++)
                {
                    message += messageArray[i];
                    message += " ";
                }

                if (messageArray[1].Equals("say", StringComparison.OrdinalIgnoreCase))
                {
                    Response response = request.CreateResponse(ResponseType.Public, message);
                    response.ResponseType = ResponseType.Public;
                    response.Channel = target;
                    ModuleManagementContainer.HandleResponse(response);
                }
                else if (messageArray[1].Equals("do", StringComparison.OrdinalIgnoreCase))
                {
                    Response response = request.CreateResponse(ResponseType.Private, "Do is currently unimplimented :(");
                    ModuleManagementContainer.HandleResponse(response);
                }
            }
        }
    }
}