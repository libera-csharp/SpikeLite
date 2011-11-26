/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.Domain.Model.Authentication
{
    /// <summary>
    /// This enumeration (temporarily) stands in for having an expression for our cloaks. By this we can tell
    /// how we should be matching the hosts - full requiring a complete match, start is akin to String.StartsWith,
    /// end is the inverse and contains requires the fragment to be anywhere in the hostmask.
    /// </summary>
    public enum HostMatchType
    {
        /// <summary>
        /// Do a literal string match.
        /// </summary>
        Full = 0,

        /// <summary>
        /// Do a String.StartsWith().
        /// </summary>
        Start = 1,

        /// <summary>
        /// Do a String.EndsWith().
        /// </summary>
        End = 2,

        /// <summary>
        /// Do a String.Contains().
        /// </summary>
        Contains = 3
    }
}