/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.Communications;
using System.Reflection;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.About
{
    public class AboutModule : ModuleBase
    {
        public override void HandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                && request.RequestSourceType == RequestSourceType.Public
                && request.Message.Equals("~about", StringComparison.OrdinalIgnoreCase))
            {
                SendResponse(request);
            }
        }

        private void SendResponse(Request request)
        {
            var message = string.Format("{0} (V{1}) is the ##csharp irc bot on freenode.net", 
                                           NetworkConnectionInformation.BotNickname, 
                                           Assembly.GetEntryAssembly().GetName().Version.ToString(2));

            var response = request.CreateResponse(ResponseType.Private, message);
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "====================================";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "IRC: To get help with the bot type '~Help' in private message with the bot.";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "Wiki: http://wiki.freenode-csharp.net/wiki/";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "====================================";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "But there's no sense crying over every mistake. You just keep on trying till you run out of cake.";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "Go make some new disaster. That's what I'm counting on. You're someone else's problem. Now I only want you gone.";
            ModuleManagementContainer.HandleResponse(response);
        }
    }
}