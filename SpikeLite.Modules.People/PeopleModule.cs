/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Globalization;
using Cadenza.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using SpikeLite.Communications;
using SpikeLite.Domain.Model.Authentication;
using SpikeLite.Domain.Model.People;
using SpikeLite.Domain.Persistence.People;

// TODO [Kog 06/19/2011] : This is just a preliminary prototype, come back and smoothe this over.
// TODO [Kog 07/17/2011] : Need some sort of filterable UI for this. Perhaps we should actually embed XSP after all, and do some quick UI.

namespace SpikeLite.Modules.People
{
    /// <summary>
    /// Provides a module to manipulate facts about people. 
    /// </summary>
    public class PeopleModule : ModuleBase
    {
        /// <summary>
        /// Holds the valid patterns for using this module.
        /// </summary>
        private const string _regexPattern = @"(~\w+)\s(\S+)\s?(.*)?";

        /// <summary>
        /// Holds a pre-compiled matcher.
        /// </summary>
        private static readonly Regex _regex = new Regex(_regexPattern);

        /// <summary>
        /// Holds an injected <see cref="IPeopleDao"/> that we can use for manipulating <see cref="Person"/> objects accordingly.
        /// </summary>
        public IPeopleDao PersonDao { get; set; }

        /// <summary>
        /// Holds the trigger we use: like "warn" or "ban" or so on. This allows us to use a prototype-like pattern for creating new
        /// people modules.
        /// </summary>
        public string Trigger { get; set; }        

        public override void HandleRequest(Request request)
        {
            // Make sure they have access and the message is meant for us.
            if (request.RequestFrom.AccessLevel >= AccessLevel.Public && request.Message.StartsWith(Trigger, true, CultureInfo.InvariantCulture))
            {
                // Parse what they sent us, make sure it's valid.
                var expressionMatch = _regex.Match(request.Message);

                // If they just passed us one atom (ie ~warn) - we won't even match. We'll just fall through to the next block.
                if (expressionMatch.Success)
                {
                    var command = expressionMatch.Groups[1].Value.Substring(1);
                    var name = expressionMatch.Groups[2].Value;
                    var normalizedName = name.ToUpperInvariant();
                    var description = expressionMatch.Groups[3].Value;

                    // OK, so we know we've at least got a person. Let's look them up. 
                    var target = PersonDao.CreateOrFindPerson(normalizedName);
                    var response = request.CreateResponse(ResponseType.Public, String.Format("No factoids of type {0} found for {1}.", command, name));

                    // If we've got a description, they're added to said person.
                    if (description.Length > 0)
                    {
                        var factoid = new PersonFactoid
                        {
                            Description = description,
                            CreationDate = DateTime.UtcNow,
                            Person = target,
                            Type = command,
                            CreatedBy = request.Nick
                        };

                        target.Factoids.Add(factoid);
                        PersonDao.SaveFactoids(target);

                        response = request.CreateResponse(ResponseType.Public, "Factoid saved.");
                    }
                    else
                    {
                        const int maxFactCount = 5;
                        var matchingFacts = target.Factoids.Where(x => x.Type.Equals(command, StringComparison.InvariantCultureIgnoreCase));
                        var factCount = matchingFacts.Count();

                        if (factCount > 0)
                        {
                            response = request.CreateResponse(ResponseType.Public, String.Format("Found {0} factoid{1} of type {2} for {3}{4}: {5}",
                                                                                                 factCount, (factCount == 1) ? String.Empty : "s", 
                                                                                                 command, 
                                                                                                 name, 
                                                                                                 (factCount > maxFactCount) ? String.Format(" (showing the first {0})", Math.Min(factCount, maxFactCount)) : string.Empty, 
                                                                                                 matchingFacts.Reverse()
                                                                                                              .Take(maxFactCount)
                                                                                                              .Select(x => String.Format("[{0} at {1} by {2}]", x.Description, x.CreationDate.ToString("MM/dd/yyyy H:mm:ss UTC"), x.CreatedBy))
                                                                                                              .Implode(", ")));
                        }                        
                    }

                    ModuleManagementContainer.HandleResponse(response);
                }
                else
                {
                    var response = request.CreateResponse(ResponseType.Public, string.Format("Invalid request, please try ~help {0}", Name));
                    ModuleManagementContainer.HandleResponse(response);
                }
            }
        }
    }
}
