/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2012 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.IPC.WebHost.Transport
{
    /// <summary>
    /// Provides a transport model version of <see cref="Domain.Model.Authentication.KnownHost"/>.
    /// </summary>
    public class KnownHost
    {
        public long Id { get; set; }

        public string HostMask { get; set; }

        public HostMatchType HostMatchType { get; set; }

        public AccessLevel AccessLevel { get; set; }

        public KnownHostMetaDatum[] MetaData { get; set; }

        public string AccessToken { get; set; }

        public DateTime? AccessTokenIssueTime { get; set; }

        public DateTime? AccessTokenExpiration { get; set; }

        public String EmailAddress { get; set; }

        public AccessFlag[] AccessFlags { get; set; }
    }
}
