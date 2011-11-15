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
    /// This enumeration is our current mechanism for accounting for access levels within the bot.
    /// </summary>
    /// 
    /// <remarks>
    /// The access levels here are a little bit confusing. At present we only use none (the user is not a regular),
    /// public (the user is a regular) and permanent admin (the user is a developer).
    /// </remarks>
    public enum AccessLevel
    {
        /// <summary>
        /// This user is not authorized to do anything.
        /// </summary>
        /// 
        /// <remarks>
        /// While we do not, at present, do anything with this access level the plan is to eventually allow 
        /// non-regulars to be able to send requests that are answered via PM and throttled with extreme prejudice.
        /// </remarks>
        None = 0,

        /// <summary>
        /// This user is authorized to talk to the bot via private messages (PRIVMSG). This is supposed to account
        /// only for 1:1 PRIVMSGs and not channel-targeted messages.
        /// </summary>
        Private = 1,

        /// <summary>
        /// This user is authorized to talk to the bot via public messages as well as private messages (either 
        /// 1:1 or 1:channel PRIVMSG).
        /// </summary>
        Public = 2,

        /// <summary>
        /// This user is an administrator and may do a limited set of maintenance actions on the bot.
        /// </summary>
        Admin = 3,

        /// <summary>
        /// This user is akin to root, and may do anything that the bot can be made to do.
        /// </summary>
        Root = 4
    }
}
