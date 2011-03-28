/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Text;
using Cadenza.Collections;
using SpikeLite.Communications;
using System.Linq;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Modules.Help
{
    /// <summary>
    /// This module provides help on the set of loaded modules. You can either query it with no target module, or attempt to look
    /// up help for a specific module.
    /// </summary>
    public class HelpModule : ModuleBase
    {
        public override void HandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public)
            {
                string[] messageArray = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (messageArray[0].Equals("~help", StringComparison.OrdinalIgnoreCase))
                {
                    // A user has asked for help, not specifying a module. List what we've got.
                    if (messageArray.Length == 1)
                    {
                        SendHelpResponses(ModuleManagementContainer.Modules.Where(module => module.Name.Equals("Help")).First(), request);
                        SendModulesListResponse(request);
                    }
                    // A user has asked for help regarding a specific module. Attempt to display help for it.
                    else if (messageArray.Length == 2)
                    {
                        string moduleName = messageArray[1];
                        IModule targetModule = ModuleManagementContainer.Modules.Where(module => 
                                                                                       module.Name.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        // We know what that module is!
                        if (null != targetModule && targetModule.RequiredAccessLevel <= request.RequestFrom.AccessLevel)
                        {
                            SendHelpResponses(targetModule, request);
                        }
                        // What? What module is that you say? Quick lads, play dumb!
                        else
                        {
                            SendModuleNotFoundResponse(moduleName, request);
                        }
                    }
                    // A user has asked us for something we don't understand. Tell them to try again.
                    else
                    {
                        SendIncorrectRequestSyntaxResponse(request);
                        SendHelpResponses(ModuleManagementContainer.Modules.Where(module => module.Name.Equals("Help")).First(), request);
                        SendModulesListResponse(request);
                    }
                }
            }
        }

        #region Utility Methods for sending messages

        /// <summary>
        /// Sends a help response for a given module.
        /// </summary>
        /// 
        /// <param name="module">The module to send information about.</param>
        /// <param name="request">The incoming message to respond to.</param>
        private void SendHelpResponses(IModule module, Request request)
        {
            Response response = request.CreateResponse(ResponseType.Private);
            
            response.Message = "Module Name: " + module.Name;
            ModuleManagementContainer.HandleResponse(response);

            response.Message = "Module Description: " + module.Description;
            ModuleManagementContainer.HandleResponse(response);

            response.Message = "Module Instructions: " + module.Instructions;
            ModuleManagementContainer.HandleResponse(response);
        }

        /// <summary>
        /// Sends a list of all module names loaded.
        /// </summary>
        /// 
        /// <param name="request">The incoming message to respond to.</param>
        private void SendModulesListResponse(Request request)
        {
            // TODO: Kog 07/06/2009 - This is a quick hack to get the help system working. Make it more awesome after the next commit.
            StringBuilder modulesList = new StringBuilder();
            ModuleManagementContainer.Modules.Where(module => module.Name != "Help"
                                                                                  &&
                                                                                  module.RequiredAccessLevel <=
                                                                                  request.RequestFrom.AccessLevel).
                                              ForEach(x => { modulesList.Append(x.Name); modulesList.Append(", "); });


            Response response = request.CreateResponse(ResponseType.Private);
            response.Message = "Modules list: " + modulesList.ToString().Trim().Trim(',');
            ModuleManagementContainer.HandleResponse(response);
        }

        /// <summary>
        /// Tell the user that we have not found the specific module requested.
        /// </summary>
        /// 
        /// <param name="moduleName">The name of the module the user wishes help for.</param>
        /// <param name="request">The incoming message to respond to.</param>
        private void SendModuleNotFoundResponse(string moduleName, Request request)
        {
            Response response = request.CreateResponse(ResponseType.Private);
            response.Message = string.Format("Module '{0}' could not be found.", moduleName);
            ModuleManagementContainer.HandleResponse(response);
        }

        /// <summary>
        /// Tell the user their help request was phrased in an invalid manner.
        /// </summary>
        /// 
        /// <param name="request">The incoming message to respond to.</param>
        private void SendIncorrectRequestSyntaxResponse(Request request)
        {
            Response response = request.CreateResponse(ResponseType.Private);
            response.Message = "Request was not in the correct syntax.";
            ModuleManagementContainer.HandleResponse(response);
        }

        #endregion 
    }
}