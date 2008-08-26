/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using SpikeLite.AccessControl;

namespace SpikeLite.Modules
{
    /// <summary>
    /// This attribute allows us to decorate a class, marking it as something eligible for loading.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ModuleAttribute : Attribute
    {
        #region Fields

        /// <summary>
        /// Holds the name of our module, such as "Google Search Module."
        /// </summary>
        private string _name;

        /// <summary>
        /// Holds a short summary of what the module does, such as "Searches for things on google."
        /// </summary>
        private string _description;

        /// <summary>
        /// Holds a brief summary on how to use the module. The grammar is mostly ad-hoc, but includes conventions
        /// such as ~<![CDATA[<name>]]> [optional param].
        /// </summary>
        private string _instructions;

        /// <summary>
        /// The <see cref="AccessLevel"/> required to run the command.
        /// </summary>
        private AccessLevel _accessLevel;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the associated module. This would be something like "Google search module."
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the description of the associated module. This would be something like "Searches the internet."
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets a short summary of usage instructions. This would be something like "~google [topic]."
        /// </summary>
        public string Instructions
        {
            get { return _instructions; }
            set { _instructions = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="AccessLevel"/> required to run the module.
        /// </summary>
        public AccessLevel AccessLevel
        {
            get { return _accessLevel; }
            set { _accessLevel = value; }
        }

        #endregion

        /// <summary>
        /// Constructs an attribute that notifies our module loader that we're to be loaded as an implementation of
        /// <see cref="SpikeLite.Modules.ModuleBase"/>.
        /// </summary>
        /// 
        /// <param name="name">The name of our module.</param>
        /// <param name="description">A short description of our module.</param>
        /// <param name="instructions">A short summary of how to use our module.</param>
        /// <param name="accessLevel">The minimum access requirement for using our module.</param>
        public ModuleAttribute(string name, string description, string instructions, AccessLevel accessLevel)
        {
            _name = name;
            _description = description;
            _instructions = instructions;
            _accessLevel = accessLevel;
        }
    }
}