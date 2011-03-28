/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
namespace SpikeLite.Domain.Model.Karma
{
    /// <summary>
    /// An entity for a user's Karma
    /// </summary>
    public class KarmaItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the PKEY used by the underlying data store
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// Gets or sets the user's name
        /// </summary>
        public virtual string UserName { get; set; }

        /// <summary>
        /// Gets or sets the karmic level of a given user
        /// </summary>
        public virtual int KarmaLevel { get; set; }

        #endregion

        #region Overridden behavior

        /// <summary>
        /// Provide pretty printing
        /// </summary>
        /// 
        /// <returns>A formatted string for display, that looks like "Bob has a karma of 999"</returns>
        public override string ToString()
        {
            return string.Format("{0} has a karma of {1}", UserName.Trim(), KarmaLevel);
        }

        #endregion
    }
}