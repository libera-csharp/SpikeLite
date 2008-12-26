/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.Puppet
{
    [Module("Puppet Module", "NA", "NA", AccessLevel.Root)]
    public class PuppetModule : ModuleBase
    {
        protected override void InternalHandleRequest(Request request)
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
                    response.ResponseType = ResponseType.Public; //force public response
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