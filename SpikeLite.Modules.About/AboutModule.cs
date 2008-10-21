/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using System.Reflection;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules.About
{
    [Module("About", "Provides information about the bot.", "Usage Syntax: ~about", AccessLevel.Public)]
    public class AboutModule : ModuleBase
    {
        #region Methods

        #region InternalHandleRequest
        protected override void InternalHandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                && request.RequestType == RequestType.Public
                && request.Message.Equals("~about", StringComparison.OrdinalIgnoreCase))
            {
                SendResponse(request);
            }
        }
        #endregion

        #region SendResponse
        protected void SendResponse(Request request)
        {
            string message = string.Format("{0} (V{1}) is the ##csharp irc bot on freenode.net", Configuration.Networks["FreeNode"].BotNickname, Assembly.GetEntryAssembly().GetName().Version.ToString(2));
            Response response = request.CreateResponse(ResponseType.Private, message);
            ModuleManagementContainer.HandleResponse(response);

            response.Message = "====================================";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "IRC: To get help with the bot type '~Help' in private message with the bot.";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "Wiki: http://www.freenode-csharp.net/wiki/SharpBot.ashx";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "====================================";
            ModuleManagementContainer.HandleResponse(response);
            response.Message = "But there's no sense crying over every mistake. You just keep on trying till you run out of cake.";
            ModuleManagementContainer.HandleResponse(response);
        }
        #endregion

        #endregion
    }
}