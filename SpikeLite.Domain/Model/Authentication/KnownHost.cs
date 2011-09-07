/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;

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

    /// <summary>
    /// This PONO represents a hostmask known to the bot. This hostmask may be a complete mask, or may be a fragment
    /// of a known hostmask (akin to using wildcards).
    /// </summary>
    public class KnownHost
    {
        #region Properties

        // TODO: Kog 01/15/2010 - We should probably swap to use the KnownHostDao and add a create method for this...

        /// <summary>
        /// Holds a backing collection for our <see cref="MetaData"/> property. This is to ensure that we never wind up with a null collection.
        /// </summary>
        private ICollection<KnownHostMetaDatum> _metaDataBacking = new List<KnownHostMetaDatum>();

        /// <summary>
        /// Holds a backing collection for our <see cref="AccessFlags"/> property. This is to ensure we never wind up with a null collection.
        /// </summary>
        private ICollection<AccessFlag> _accessFlags = new List<AccessFlag>();

        /// <summary>
        /// This should be generated and set by our ORM. This is our PKey, non-naturalized for her pleasure.
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// Gets or sets the hostmask expression that represents a known user.
        /// </summary>
        public virtual string HostMask { get; set; }

        /// <summary>
        /// Gets or sets the type of match we're doing - partial, complete, starts/endswith or contains.
        /// </summary>
        public virtual HostMatchType HostMatchType { get; set; }

        /// <summary>
        /// Gets or sets the access of the host. The default is unknown, or none (0).
        /// </summary>
        public virtual AccessLevel AccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the associated metadata. May be empty, but never null.
        /// </summary>
        public virtual ICollection<KnownHostMetaDatum> MetaData
        {
            get { return _metaDataBacking; }
            set { _metaDataBacking = value; }
        }

        // TODO [Kog 07/23/2011] : We went with 24 hour living GUIDs as tokens, which users may ask the bot for, then copy/paste.
        // TODO [Kog 07/23/2011] : In the future we may need to store these to ensure uniqueness, so we may want to change the
        // TODO [Kog 07/23/2011] : data model accordingly.

        /// <summary>
        /// Gets or sets an "access token" that the user may ask to be requested for them. This is an opaque token used for
        /// authentication/access checking purposes: when presented it may be redeemed for a set of allowed permissions. The bot
        /// modules individually may still choose to veto operations, but should respect the flags as presented. 
        /// </summary>
        public virtual string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> at which the <see cref="AccessToken"/> was issued.
        /// </summary>
        /// 
        /// <remarks>
        /// This time should be UTC and will be used to expire the token, if tokens are not immortal.
        /// </remarks>
        public virtual DateTime? AccessTokenIssueTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> at which the <see cref="AccessToken"/> will expire.
        /// </summary>
        public virtual DateTime? AccessTokenExpiration { get; set; }

        /// <summary>
        /// Contains an email address to use for the purposes of remote credentials, such as delegated auth.
        /// </summary>
        public virtual String EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the associated access flags for this user. May be empty, but never null.
        /// </summary>
        public virtual ICollection<AccessFlag> AccessFlags
        {
            get { return _accessFlags; }
            set { _accessFlags = value; }
        }

        #endregion

        // TODO: Kog 10/19/2008 - replace this with an expression instead of a hard-coded match type.

        public virtual bool Matches(string otherMask)
        {
            switch (HostMatchType)
            {
                case HostMatchType.Full:
                    return HostMask.Equals(otherMask);
                case HostMatchType.Start:
                    return otherMask.StartsWith(HostMask);
                case HostMatchType.End:
                    return otherMask.EndsWith(HostMask);
                case HostMatchType.Contains:
                    return otherMask.Contains(HostMask);
                default:
                    throw new InvalidOperationException(string.Format("Unrecognized HostMatchType {0}", HostMatchType));
            }
        }
    }
}