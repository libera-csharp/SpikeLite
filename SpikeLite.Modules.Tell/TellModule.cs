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

namespace SpikeLite.Modules.Tell
{
    [Module("Tell", "Tell allows you to direct a response from a speicifed command at a specified user.", "Usage Syntax: '~tell <NickName> <Command> <CommandArgs>' i.e. '~tell SomeUser google stuff'", AccessLevel.Public)]
    public class TellModule : ModuleBase
    {
        #region Methods

        #region InternalHandleRequest
        protected override void InternalHandleRequest(Request request)
        {
            string[] messageArray = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                && request.RequestType == RequestType.Public
                && messageArray[0].Equals("~tell", StringComparison.OrdinalIgnoreCase))
            {
                string newMessage = string.Empty;

                for (int i = 1; i < messageArray.Length; i++)
                {
                    if (i == 1)
                    {
                        request.Nick = messageArray[i];
                    }
                    else if (i == 2)
                    {
                        string command;

                        if (!messageArray[i].StartsWith("~"))
                            command = "~" + messageArray[i];
                        else
                            command = messageArray[i];

                        newMessage += command;
                        newMessage += " ";
                    }
                    else if (i > 2)
                    {
                        newMessage += messageArray[i];
                        newMessage += " ";
                    }
                }

                request.Message = newMessage.Trim();

                ModuleManagementContainer.HandleRequest(request);
            }
        }
        #endregion

        #endregion
    }
}