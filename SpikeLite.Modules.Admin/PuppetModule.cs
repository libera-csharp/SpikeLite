/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Admin
{
    public class PuppetModule : ModuleBase
    {
        public override void HandleRequest(Request request)
        {
            var messageArray = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestSourceType == RequestSourceType.Private
                && request.RequestFrom.AccessLevel >= AccessLevel.Root
                && messageArray[0].Equals("~puppet", StringComparison.OrdinalIgnoreCase)
                && messageArray.Length >= 4)
            {
                var target = messageArray[2].Trim();
                var message = string.Empty;

                for (var i = 3; i < messageArray.Length; i++)
                {
                    message += messageArray[i];
                    message += " ";
                }

                if (messageArray[1].Equals("say", StringComparison.OrdinalIgnoreCase))
                {
                    var response = request.CreateResponse(ResponseTargetType.Public, message);
                    response.ResponseTargetType = ResponseTargetType.Public;
                    response.Channel = target;
                    ModuleManagementContainer.HandleResponse(response);
                }
            }
        }
    }
}