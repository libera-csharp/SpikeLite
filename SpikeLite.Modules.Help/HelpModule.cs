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

namespace SpikeLite.Modules.Help
{
    [Module("Help", "Help provides information about the modules and how to use them.", "Usage Syntax: ~help <module name>", AccessLevel.Public)]
    public class HelpModule : ModuleBase
    {
        #region Methods

        #region InternalHandleRequest
        protected override void InternalHandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public)
            {
                string[] messageArray = request.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (messageArray[0].Equals("~help", StringComparison.OrdinalIgnoreCase))
                {
                    if (messageArray.Length == 1)
                    {
                        SendHelpResponses(ModuleManagementContainer.ModuleAttributesByName["help"], request);
                        SendModulesListResponse(request);
                    }
                    else if (messageArray.Length == 2)
                    {
                        string moduleName = messageArray[1];

                        if (ModuleManagementContainer.ModulesByName.ContainsKey(moduleName)
                            && ModuleManagementContainer.ModuleAttributesByName[moduleName].AccessLevel <= request.RequestFrom.AccessLevel)
                        {
                            SendHelpResponses(ModuleManagementContainer.ModuleAttributesByName[moduleName], request);
                        }
                        else
                            SendModuleNotFoundResponse(moduleName, request);
                    }
                    else
                    {
                        SendIncorrectRequestSyntaxResponse(request);
                        SendHelpResponses(ModuleManagementContainer.ModuleAttributesByName["Help"], request);
                        SendModulesListResponse(request);
                    }
                }
            }
        }
        #endregion

        #region SendHelpResponses
        protected void SendHelpResponses(ModuleAttribute moduleAttribute, Request request)
        {
            Response response = request.CreateResponse(ResponseType.Private);
            
            response.Message = "Module Name: " + moduleAttribute.Name;
            ModuleManagementContainer.HandleResponse(response);

            response.Message = "Module Description: " + moduleAttribute.Description;
            ModuleManagementContainer.HandleResponse(response);

            response.Message = "Module Instructions: " + moduleAttribute.Instructions;
            ModuleManagementContainer.HandleResponse(response);
        }
        #endregion

        #region SendModulesListResponse
        protected void SendModulesListResponse(Request request)
        {
            string moduleList = string.Empty;

            foreach (string moduleName in ModuleManagementContainer.ModuleAttributesByName.Keys)
            {
                if (ModuleManagementContainer.ModuleAttributesByName[moduleName].AccessLevel <= request.RequestFrom.AccessLevel 
                    && !moduleName.Equals("Help"))
                {
                    moduleList += moduleName;
                    moduleList += ", ";
                }
            }

            Response response = request.CreateResponse(ResponseType.Private);
            response.Message = "Modules list: " + moduleList.Trim().Trim(',');
            ModuleManagementContainer.HandleResponse(response);
        }
        #endregion

        #region SendModuleNotFoundResponse
        protected void SendModuleNotFoundResponse(string moduleName, Request request)
        {
            Response response = request.CreateResponse(ResponseType.Private);
            response.Message = string.Format("Module '{0}' could not be found.", moduleName);
            ModuleManagementContainer.HandleResponse(response);
        }
        #endregion

        #region SendIncorrectRequestSyntaxResponse
        protected void SendIncorrectRequestSyntaxResponse(Request request)
        {
            Response response = request.CreateResponse(ResponseType.Private);
            response.Message = "Request was not in the correct syntax.";
            ModuleManagementContainer.HandleResponse(response);
        }
        #endregion

        #endregion
    }
}