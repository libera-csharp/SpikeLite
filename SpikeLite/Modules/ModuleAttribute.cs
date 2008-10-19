/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.Persistence.Authentication;

namespace SpikeLite.Modules
{
    /// <summary>
    /// This attribute allows us to decorate a class, marking it as something eligible for loading.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ModuleAttribute : Attribute
    {
        #region Fields

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the associated module. This would be something like "Google search module."
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the associated module. This would be something like "Searches the internet."
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a short summary of usage instructions. This would be something like "~google [topic]."
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AccessLevel"/> required to run the module.
        /// </summary>
        public AccessLevel AccessLevel { get; set; }

        #endregion

        /// <summary>
        /// Constructs an attribute that notifies our module loader that we're to be loaded as an implementation of
        /// ModuleBase.
        /// </summary>
        /// 
        /// <param name="name">The name of our module.</param>
        /// <param name="description">A short description of our module.</param>
        /// <param name="instructions">A short summary of how to use our module.</param>
        /// <param name="accessLevel">The minimum access requirement for using our module.</param>
        public ModuleAttribute(string name, string description, string instructions, AccessLevel accessLevel)
        {
            Name = name;
            Description = description;
            Instructions = instructions;
            AccessLevel = accessLevel;
        }
    }
}