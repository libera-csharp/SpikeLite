/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Collections.Generic;

namespace SpikeLite.Communications.Irc.Configuration
{
    /// <summary>
    /// This class represents a Server PONO. A server belongs to one, and only one network at a time, and
    /// contains a minimum of information. The bot should only ever connect to one server within a network at
    /// a time, moving down the list upon disconnection or inability to connect. The max retries is a per-server
    /// value.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Gets or sets the host of the server. May be either a quad-dotted IPv4 address or a standard domain name.
        /// May not be null.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port number to attempt to connect to. If no value is specified, 6667 is used.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the order in which this server should be used. If no value is specified, 0 is used. 
        /// </summary>
        /// 
        /// <remarks>
        /// Upon connection/reconnection, we attempt to join our sorted list of servers for the network. If you have
        /// a list of servers A (weight 1), B (weight 2), C (weight 3), the network connection will attempt network
        /// C first until it either connects, or reaches the max retry count. If all servers are exhausted for 
        /// a network, the bot ceases attempting to connect to that network until otherwise instructed.
        /// </remarks>
        public int? Weight { get; set; }

        /// <summary>
        /// Gets or sets the list of channels the bot should join upon the given server. This list will be sorted
        /// again at runtime by the algorithm discussed on the Weight property. This list should not be null, and
        /// if empty the bot will be on the network (and able to respond to NOTICE or PRIVMSG), but not in any
        /// channels.
        /// </summary>
        /// 
        /// <remarks>
        /// In the event that this list is non-null, but empty, the bot will connect to the given server and sit
        /// there until disconnected from either side. It will respond to both NOTICE and PRIVMSG (of the 1:1 
        /// variety), but this may have side effects. Not all commands support a PRIVATE mode (responding in a
        /// single-target mode).
        /// </remarks>
        public IList<Channel> ChannelList { get; set; }
    }
}