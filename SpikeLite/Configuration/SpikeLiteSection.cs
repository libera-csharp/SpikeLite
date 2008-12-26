/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Configuration;

namespace SpikeLite.Configuration
{
    public class SpikeLiteSection : ConfigurationSection
    {
        #region Static Fields

        private static readonly ConfigurationProperty _networks;
        private static readonly ConfigurationPropertyCollection _properties;

        private static SpikeLiteSection _section;

        #endregion
         
        #region Properties

        [ConfigurationProperty("networks", IsRequired=true)]
        public NetworkElementCollection Networks
        {
            get { return (NetworkElementCollection)base[_networks]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        static SpikeLiteSection()
        {
            _networks = new ConfigurationProperty("networks", typeof(NetworkElementCollection), null, ConfigurationPropertyOptions.IsRequired);
            _properties = new ConfigurationPropertyCollection {_networks};
        }

        #region GetSection Pattern

        /// <summary>
        /// Gets the configuration section using the default element name.
        /// </summary>
        public static SpikeLiteSection GetSection()
        {
            return GetSection("spikeLite");
        }

        /// <summary>
        /// Gets the configuration section using the specified element name.
        /// </summary>
        public static SpikeLiteSection GetSection(string definedName)
        {
            if (_section == null)
            {
                _section = ConfigurationManager.GetSection(definedName) as SpikeLiteSection;

                if (_section == null)
                {
                    throw new ConfigurationErrorsException(string.Format("The <{0}> section is not defined in the .config file.", definedName));
                }
            }

            return _section;
        }

        #endregion

    }
}