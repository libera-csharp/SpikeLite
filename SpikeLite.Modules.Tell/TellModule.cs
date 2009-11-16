/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Tell
{
    public class TellModule : ModuleBase
    {
        #region InternalHandleRequest

        public override void HandleRequest(Request request)
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
                        request.Addressee = messageArray[i];
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
    }
}