/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Communications;
using SpikeLite.Persistence.Authentication;
using SpikeLite.Persistence.Karma;
using System.Text.RegularExpressions;

namespace SpikeLite.Modules.Karma
{
    /// <summary>
    /// A Karma storage module, based on ideas from JavaBot.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para>
    /// Karma is one way of measuring a user's value to the community. One may increment or decrement the karma
    /// of another if they feel the person's actions merrit it. One cannot affect their own karma, or there will
    /// be negative consequences. Lastly, you may inquire as to the karma status of anyone.
    /// </para>
    /// 
    /// <para>
    /// This module is usable by anyone in the channel.
    /// </para>
    /// 
    /// </remarks>
    [Module("Karma", "Stores user's Karma", "~karma <NickName>++ to increment a users karma. ~karma <NickName>-- to decrement a users karma. ~karma <NickName> to query a user's karma.", AccessLevel.Public)]
    public class KarmaModule : ModuleBase
    {
        #region Data Members

        /// <summary>
        /// Holds the valid patterns for our karma regex, with some groupings for easy parsing.
        /// </summary>
        private const string _regexPattern = @"~karma\s([A-Za-z0-9]+)((\-\-)|(\+\+))?";

        /// <summary>
        /// Precompile our matcher.
        /// </summary>
        private static readonly Regex _karmaRegex = new Regex(_regexPattern);

        /// <summary>
        /// This stores our Karma DAO that we use for finding/persisting Karma. This takes the place of what
        /// were previously static methods.
        /// </summary>
        private KarmaDao _karmaDao;

        #endregion

        protected override void InternalInitModule() 
        {
            // Make sure we construct our Karma DAO for later.
            _karmaDao = new KarmaDao(Persistence);
        }

        /// <summary>
        /// See if we want to digest this message, and if so take action.
        /// </summary>
        /// 
        /// <param name="request">A message</param>
        /// 
        /// <remarks>
        /// <para>
        /// We do rather inelegant, or perhaps more accurately inprecise, handling of our mapping here.
        /// We assume that the format is ~karma (nick)++/--. We assume that -+ or other things that are not
        /// alphanumeric after the nickname (as defined by an alphanumeric string) are invalid input, and do
        /// a karma lookup.
        /// </para>
        /// 
        /// <para>
        /// The alternative to doing this is a lot of funky corner logic which really doesn't need to happen.
        /// This module is currently not open to the public since we have no throttling on public messages, and
        /// we don't want users to spam us offline.
        /// </para>
        /// </remarks>
        protected override void InternalHandleRequest(Request request)
        {
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public)
            {
                Match expressionMatch = _karmaRegex.Match(request.Message);

                if (expressionMatch.Success)
                {
                    String nick = expressionMatch.Groups[1].Value;
                    String op = expressionMatch.Groups[2].Value;

                    Response response;

                    // Make sure the sender isn't messing with their own karma.
                    if (nick.Equals(request.Nick, StringComparison.OrdinalIgnoreCase) && !String.IsNullOrEmpty(op))
                    {
                        response = request.CreateResponse(ResponseType.Public, string.Format("{0}, toggling your own karma is very uncool.", request.Nick));
                        ModuleManagementContainer.HandleResponse(response);
                    }
                    else
                    {
                        // Attempt to look the user up.
                        KarmaItem karma = _karmaDao.findKarma(nick) ?? new KarmaItem {KarmaLevel = 0, UserName = nick};

                        // If they're doing anything more than looking...
                        if (!String.IsNullOrEmpty(op))
                        {
                            if (op.Equals("--"))
                            {
                                karma.KarmaLevel--;
                            }
                            else if (op.Equals("++"))
                            {
                                karma.KarmaLevel++;
                            }

                            _karmaDao.saveKarma(karma);
                        }

                        response = request.CreateResponse(ResponseType.Public, karma.ToString());
                        ModuleManagementContainer.HandleResponse(response);
                    }
                }
            }
        }
    }
}