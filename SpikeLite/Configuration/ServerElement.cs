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
    public class ServerElement : ConfigurationElement
    {
        #region Static Fields

        private static ConfigurationProperty _hostnameOrIP;
        private static ConfigurationProperty _port;

        private static ConfigurationPropertyCollection _properties;
        
        #endregion

        #region Properties

        [ConfigurationProperty("hostnameOrIP", IsRequired = true)]
        public string HostnameOrIP
        {
            get { return (string)base[_hostnameOrIP]; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)base[_port]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        static ServerElement()
        {
            _hostnameOrIP = new ConfigurationProperty("hostnameOrIP", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _port = new ConfigurationProperty("port", typeof(int), null, ConfigurationPropertyOptions.IsRequired);

            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_hostnameOrIP);
            _properties.Add(_port);
        }
    }
}