/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;

namespace SpikeLite.Communications.IRC
{
    /// <summary>
    /// This class defines a network PONO. A network is composed of 0 or more servers and contains some
    /// network-level properties such as a nickname, an authentication strategy etc.
    /// </summary>
    public class Network
    {
        // TODO: Kog 12/26/2008 - In order to support multiple networks, add some strategy here for authentication,
        // TODO:                  supporting nickserv based auth, no-auth (efnet style) or other account based services.

        /// <summary>
        /// Gets or sets the name of this network. May be null.
        /// </summary>
        public string NetworkName { get; set; }

        /// <summary>
        /// Gets or sets the nickname of the bot upon the network, as specified in RFC1459. May not be null.
        /// </summary>
        public string BotNickname { get; set; }

        /// <summary>
        /// Gets or sets the "real name" of the bot upon the network, as specified in RFC1459. May not be null.
        /// </summary>
        public string BotRealname { get; set; }

        /// <summary>
        /// Gets or sets the user name of the bot upon the network, as specified by RFC1459. May not be null.
        /// </summary>
        public string BotUsername { get; set; }

        /// <summary>
        /// Gets or sets the password used for the bot upon the network. If null, we will not attempt ao authenticate.
        /// </summary>
        /// 
        /// <remarks>
        /// SASL and other mechanisms are not currently supported. Only plain messaging based authentication such
        /// as used by undernet-style services (NickServ, AuthServ, X2, AccountServ etc.).
        /// </remarks>
        public string AccountPassword { get; set; }

        /// <summary>
        /// Gets or sets the list of servers to be used for this network. This list will be sorted
        /// again at runtime by the algorithm discussed on the Weight property. This list must not be null, but
        /// may be empty. If the list is empty the bot will not connect to any servers for this network. Doing so
        /// effectively disables that given network.
        /// </summary>
        public IList<Server> ServerList { get; set; }
    }
}