using SpikeLite.Modules;
using SpikeLite.AccessControl;
using SpikeLite.Communications;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Sharkbite.Irc;
using System.Linq;
using System.Text;

namespace SpikeLite.Spammy
{
    /// <summary>
    /// This module was written in response to a specific spammer who seems to be spamming the channel with
    /// the same message except the prefix is always a 4 digit number. So, this listener takes care of that.
    /// Eventually it needs some sort of notification, or some sort of action mechanism. It also should probably
    /// be adapted to other sorts of attacks and maybe given some sort of date range expression mechanism.
    /// 
    /// This will do for now.
    /// </summary>
    /// 
    /// <remarks>
    /// This module is not meant to be run on the standard bot. There is no way of clearing the spamlog, and the bot
    /// has high uptime. Please do not activate this module.
    /// </remarks>
    //[Module("Listener", "Test module, does nothing.", "You cannot use this module.", AccessLevel.Public)]
    public class DigitListener : ModuleBase
    {
        #region Data Members

        // TODO: Kog 08/25/2008 - Do we want to persist this? Do we want a strategy pattern here?

        /// <summary>
        /// Keep track of our spam incidents.
        /// </summary>
        private Dictionary<string, Spammer> _spamLog;

        /// <summary>
        /// We match something that starts with 4 digits, then has a whitespace, then has a bunch
        /// of random characters. We save the characters, but do no more.
        /// </summary>
        private static readonly string _trigger = @"^\d{4}\s(.*)";

        /// <summary>
        /// A regular expression object for match checking and grouping.
        /// </summary>
        private static readonly Regex _triggerRegex = new Regex(_trigger);

        /// <summary>
        /// When we're clearing our spam cache, make sure we lock it.
        /// </summary>
        private static readonly object _mutex = new object();

        #endregion 

        /// <summary>
        /// Make sure we initialize everything we need for spam hunting. Oh yes.
        /// </summary>
        public DigitListener()
        {
            _spamLog = new Dictionary<string, Spammer>();
        }

        /// <summary>
        /// This module is intentionally undocumented from a public user's perspective. 
        /// </summary>
        /// 
        /// <param name="request"></param>
        protected override void InternalHandleRequest(Request request)
        {
            #region Spam Filtration

            Match expressionMatch = _triggerRegex.Match(request.Message);

            // If an unknown user has set off our spam sensor we are green for go.
            if (expressionMatch.Success && request.RequestFrom.AccessLevel < AccessLevel.Public)
            {
                Spammer spammer;

                // It's a brand new spammer!
                if (!_spamLog.TryGetValue(request.Nick, out spammer))
                {
                    spammer = new Spammer(request.Nick);
                    spammer.SpamMessage = expressionMatch.Groups[1].Value;

                    // Make sure we're cached for next round.
                    _spamLog.Add(request.Nick, spammer);

                    // Notify ##csharp-dev
                    ModuleManagementContainer.HandleResponse(
                        new Response("##csharp-dev", "Kog", ResponseType.Public, 
                                     string.Format("Detected a new spammer: {0}", request.Nick))
                    );

                    // Notify the needful
                    ModuleManagementContainer.HandleResponse(
                        new Response("##csharp-dev", "Kog", ResponseType.Private,
                                     string.Format("Detected a new spammer: {0}", request.Nick))
                    );

                    ModuleManagementContainer.HandleResponse(
                        new Response("##csharp-dev", "Kog|Work", ResponseType.Private,
                                     string.Format("Detected a new spammer: {0}", request.Nick))
                    );
                }

                // Increment ze clones!
                spammer.incrementSpam();
            }

            #endregion 

            #region Administrator Only Commands

            // Request information about a specific spammer.
            if (request.Message.StartsWith("~spam") && request.RequestFrom.AccessLevel == AccessLevel.PermanentAdmin)
            {
                string target = request.Message.Substring(6).Trim();
                string message = string.Empty;

                message = _spamLog.ContainsKey(target) ? _spamLog[target].ToString() : "No such spammer";

                ModuleManagementContainer.HandleResponse(
                    new Response(request.Channel, request.Nick, ResponseType.Public, message)
                );
            }

            // TODO: Kog 08/25/2008 - add a list of hostmasks and CTCP/Version info here.

            // Request a list of all spammers.
            if (request.Message.Equals("~allspam") && request.RequestFrom.AccessLevel == AccessLevel.PermanentAdmin)
            {
                int spamCount = _spamLog.Keys.Count;
                string responseString = string.Format("{0} spammer{1} found{2} {3}", 
                                                      spamCount, 
                                                      (spamCount != 1) ? "s" : "",
                                                      (spamCount > 0) ? ":" : ".",
                                                      string.Join(",", _spamLog.Keys.ToArray<string>()));

                ModuleManagementContainer.HandleResponse(
                    new Response(request.Channel, request.Nick, ResponseType.Public, responseString)
                );
            }

            if (request.Message.Equals("~clearspam") && request.RequestFrom.AccessLevel == AccessLevel.PermanentAdmin)
            {
                lock (_mutex)
                {
                    _spamLog = new Dictionary<string, Spammer>();
                }

                ModuleManagementContainer.HandleResponse(
                    new Response(request.Channel, request.Nick, ResponseType.Public, "Spam log cleared.")
                );
            }

            #endregion 
        }

        #region Spam Tracking

        // TODO: Kog 08/25/2008 - add a spamlog clear. Make permadmin.

        /// <summary>
        /// A very simple, temporary inner class containing information about spam events. This class
        /// assumes that every instance of the message is similar. Please see the parent class documentation
        /// for more.
        /// </summary>
        private class Spammer
        {
            #region Properties

            public string Nick
            {
                get; set;
            }

            public int SpamCount
            {
                get; set;
            }

            public string SpamMessage
            {
                get; set;
            }

            public List<DateTime> SpamEvents
            {
                get; set;
            }

            #endregion 

            public Spammer(string nick)
            {
                Nick = nick;
                SpamEvents = new List<DateTime>();
            }

            public void incrementSpam()
            {
                SpamCount++;
                SpamEvents.Add(DateTime.Now);
            }

            /// <summary>
            /// Give an IRC friendly summary of what the spam is.
            /// </summary>
            /// 
            /// <returns>An IRC friendly summary of the requested spam.</returns>
            /// 
            /// <remarks>
            /// IRC friendly summary, in this case means that we return something like:
            /// 
            /// NickServ has spammed the following message 1 time(s) starting at 8/25/2008 9:57:50 PM and ending at 
            /// 8/25/2008 9:57:50 PM: ello everyone; I am trying to compile a piece of software, but keep getting 
            /// 'error: âwaddwstrâ was not declared in this scope'. Is there any way to remedy this? and u guys guys how 
            /// can i create a define which places a // into its stead? i tried #define // but that doesn't work. that 
            /// looks like a function. If it is then you need to declare it.
            /// </remarks>
            public override string ToString()
            {
                return string.Format("{0} has spammed the following message {1} time(s) starting at {2} and ending at {3}: {4}",
                                     Nick, SpamCount, SpamEvents.First<DateTime>(), SpamEvents.Last<DateTime>(), SpamMessage);
            }
        }

        #endregion 
    }
}
