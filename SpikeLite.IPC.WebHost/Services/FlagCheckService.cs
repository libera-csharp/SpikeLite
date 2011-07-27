/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using System.ServiceModel;
using SpikeLite.Domain.Model.Authentication;
using System.Linq;

namespace SpikeLite.IPC.WebHost.Services
{
    /// <summary>
    /// Provides a service that allows you to check the access flags on a given account.
    /// </summary>
    [ServiceContract(Namespace = "http://tempuri.org")]
    public interface IFlagCheckService
    {
        /// <summary>
        /// Given the header information, attempts to return the set of flags for a given user.
        /// </summary>
        /// 
        /// <returns>A collection of flags the user has been granted. May be empty, or null.</returns>
        [OperationContract]
        [SecuredOperation]
        IEnumerable<AccessFlag> GetFlags();
    }

    /// <summary>
    /// Provides a concrete implementation of <see cref="IFlagCheckService"/>.
    /// </summary>
    public class FlagCheckService : AbstractUserContextAwareService, IFlagCheckService
    {
        public IEnumerable<AccessFlag> GetFlags()
        {
            var principal = GetPrincipal();

            return null == principal ? new List<AccessFlag>() : principal.AccessFlags.ToList();
        }
    }
}
