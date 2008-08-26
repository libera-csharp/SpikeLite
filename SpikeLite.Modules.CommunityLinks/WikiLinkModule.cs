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

namespace SpikeLite.Modules.CommunityLinks
{
    [Module("WikiLink", "Returns the link to the community wiki.", "~wiki", AccessLevel.Public)]
    public class WikiLinkModule : ModuleBase
    {
        protected override void InternalHandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public
                && request.Message.Equals("~wiki", StringComparison.OrdinalIgnoreCase))
            {
                Response response = request.CreateResponse(ResponseType.Public, @"{0}, http://www.freenode-csharp.net/wiki/", request.Nick);
                ModuleManagementContainer.HandleResponse(response);
            }
        }
    }
}