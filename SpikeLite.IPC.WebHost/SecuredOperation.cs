/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net.Ext.Trace;
using SpikeLite.AccessControl;
using SpikeLite.Domain.Model.Authentication;
using System.Linq;
using Spring.Context.Support;

namespace SpikeLite.IPC.WebHost
{
    /// <summary>
    /// Provides an <see cref="IOperationInvoker"/> that can do some simple authentication for us.
    /// </summary>
    public class SecuredOperation : Attribute, IOperationInvoker, IOperationBehavior
    {
        /// <summary>
        /// Holds the invoker that we delegate to if authentication is successful.
        /// </summary>
        private IOperationInvoker _delegatedInvoker;

        /// <summary>
        /// Holds a collection of required access flags for using the resource.
        /// </summary>
        private readonly IEnumerable<string> _requiredFlags;

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private static readonly TraceLogImpl Logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(SecuredOperation));

        /// <summary>
        /// Allows for decoratorative, annotation-based security around service contracts. 
        /// </summary>
        /// 
        /// <param name="flags">An optional set of string literals for flags. May be empty, signifying that no special flags (and only a recognized account) needs to be used.</param>
        public SecuredOperation(params string[] flags)
        {
            _requiredFlags = flags;
        }

        #region IOperationInvoker

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            // If we've got no user information, throw an exception. We might actually want to tailor this behavior... 
            var principal = FindUserPrincipalContext();
            Exception error = null;

            // TODO [Kog 07/25/2011] : Perhaps more granular error logging? Probably need to refactor this anyway, especially once we figure out how to properly pass context.
            // TODO [Kog 07/26/2011] : Better error handling.

            // Check that we've got a principal that matches.
            if (null == principal)
            {
                Logger.Info("Authentication request rejected - either the credentials given were invalid, or the token was expired.");
                error = new Exception("Invalid user context.");
            }

            // Make sure said principal has the required flags.
            if (null != principal && (principal.AccessLevel != AccessLevel.Root && !RequiredFlagsSet(principal)))
            {
                Logger.InfoFormat("User {0} does not have the required flags to complete operation {1}", 
                                  principal.EmailAddress, OperationContext.Current.EndpointDispatcher.EndpointAddress);

                error = new Exception("Invalid user context.");
            }

            // If the user has failed either challange, bail.
            if (null != error)
            {
                throw error;
            }

            // Call out to our delegate. Let's stuff something into our current context though.
            using (new OperationContextScope(OperationContext.Current))
            {
                // Shove our principal into the current context, so operations can get it later.
                OperationContext.Current.IncomingMessageProperties.Add("principal", principal);

                // If we've got credentials, invoke our delegated behavior.
                return _delegatedInvoker.Invoke(instance, inputs, out outputs);                
            }
        }

        public object[] AllocateInputs()
        {
            return _delegatedInvoker.AllocateInputs();
        }

        #region Async stuff that's not supported yet.

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotImplementedException("This operation is not asynchronous.");
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotImplementedException("This operation is not asynchronous.");
        }

        public bool IsSynchronous
        {
            // We only support synchronous operations with this attribute.
            get { return true; }
        }

        #endregion

        #endregion

        #region  IOperationBehavior

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            // We want to intercept normal invocation. The rest of the methods will be called early in the runtime and are of no use.
            _delegatedInvoker = dispatchOperation.Invoker;
            dispatchOperation.Invoker = this;
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            // No-op.
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            // No-op.
        }

        public void Validate(OperationDescription operationDescription)
        {
            // No-op.
        }


        #endregion

        #region Private Implementation Behavior

        /// <summary>
        /// Attempts to find a user principal based on information in the message headers.
        /// </summary>
        /// 
        /// <returns>A <see cref="KnownHost"/> corresponding to the given email address/access token combination, if any. Will return null if no user matches.</returns>
        private static KnownHost FindUserPrincipalContext()
        {
            // Get our application context from Spring.NET.
            var ctx = ContextRegistry.GetContext();
            var authenticationModule = (IrcAuthenticationModule)ctx.GetObject("IrcAuthenticationModule");

            KnownHost user = null;

            // TODO [Kog 07/26/2011] : Make these headers configurable.

            // Try and grab the auth information out of our message headers.
            var emailAddress = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>("String", "net.freenode-csharp.auth.email");
            var accessToken = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>("String", "net.freenode-csharp.auth.token");

            // If the auth information from the headers isn't empty, let's try and find the user given the information we've got.
            if (!string.IsNullOrEmpty(emailAddress) && !string.IsNullOrEmpty(accessToken))
            {
                // Make sure the user/token combination exists, and that the token within the acceptable TTL.
                user = authenticationModule.FindHostByEmailAddress(emailAddress, accessToken, x => x.HasValue && (x.Value.Subtract(DateTime.Now.ToUniversalTime()) > TimeSpan.Zero));
            }

            return user;
        }

        /// <summary>
        /// A private utility method, ensures that the user has the flags that are required to access this resource.
        /// </summary>
        /// 
        /// <param name="host">A <see cref="host"/> to check for required flags.</param>
        /// 
        /// <returns>True if the user has all the required flags, else false.</returns>
        private bool RequiredFlagsSet(KnownHost host)
        {
            // Make sure that we've even got anything to check...
            if (null != host && null != host.AccessFlags)
            {
                // Make sure that all the required flags are present on the principal.
                return _requiredFlags.All(x => host.AccessFlags.Any(y => y.Flag.Equals(x, StringComparison.CurrentCultureIgnoreCase)));
            }

            return false;
        }

        #endregion
    }
}
