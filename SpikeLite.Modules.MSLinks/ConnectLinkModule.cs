/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using SpikeLite.AccessControl;

namespace SpikeLite.Modules.MSLinks
{
    [Module("ConnectLink", "Returns the link to microsoft connect.", "~connect", AccessLevel.Public)]
    public class ConnectLinkModule : ModuleBase
    {
        protected override void InternalHandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                && request.Message.Equals("~connect", StringComparison.OrdinalIgnoreCase))
            {
                Response response = request.CreateResponse(ResponseType.Public, @"{0}, http://connect.microsoft.com/", request.Nick);
                ModuleManagementContainer.HandleResponse(response);
            }
        }
    }
}