/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace SpikeLite.Configuration
{
    public class LicenseElement : ConfigurationElement
    {
        #region Static Fields

        private static ConfigurationProperty _applicationName;
        private static ConfigurationProperty _key;
        private static ConfigurationPropertyCollection _properties;

        #endregion

        #region Properties

        [ConfigurationProperty("applicationName", IsRequired = true)]
        public string ApplicationName
        {
            get { return (string)base[_applicationName]; }
        }

        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return (string)base[_key]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        #region Constructor

        static LicenseElement()
        {
            _applicationName = new ConfigurationProperty("applicationName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _key = new ConfigurationProperty("key", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_applicationName);
            _properties.Add(_key);
        }

        #endregion
    }
}