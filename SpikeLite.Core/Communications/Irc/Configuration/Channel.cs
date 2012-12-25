/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
namespace SpikeLite.Communications.Irc.Configuration
{
    /// <summary>
    /// This class represents an IRC channel PONO. A channel belongs to a server, although they are non-unique. A 
    /// network may be composed of multiple servers with the same set of channels, or disparate sets of channels. 
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Gets or sets the name of the channel. This must be a supported channel name on your daemon, including 
        /// the channel type (such as #, !, & etc). A valid example is ##csharp. This may not be null.
        /// </summary>
        /// 
        /// <remarks>
        /// No cleaning is done to ensure that the channel name is appropriate to the set of capabilities an IRCD
        /// exposes. For example, it's possible to have a channel type that's local only by naming the channel
        /// something like &amp;localchannel. Your IRCD may or may not support this.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an optional channel key, as required when a channel is +k. If no key is specified,
        /// we will attempt to use "", which in IRC is a null key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the order in which this channel should be joined in relation to the other channels for
        /// a given server. If no value is specified a value of 0 will be used.
        /// </summary>
        /// 
        /// <remarks>
        /// In the event a tie is found (for example: you have #a with a weight of 1 and #b with a weight of 1), 
        /// we will sort secondarily by channel name ascending.
        /// </remarks>
        public int? Weight { get; set; }
    }
}