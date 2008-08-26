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
    public class NetworkElement : ConfigurationElement
    {
        #region Static Fields

        private static ConfigurationProperty _networkName;
        private static ConfigurationProperty _botNickname;
        private static ConfigurationProperty _botRealName;
        private static ConfigurationProperty _botUserName;
        private static ConfigurationProperty _nickServPassword;
        private static ConfigurationProperty _servers;
        private static ConfigurationProperty _channels;

        private static ConfigurationPropertyCollection _properties;

        #endregion

        #region Properties

        [ConfigurationProperty("networkName", IsRequired = true)]
        public string NetworkName
        {
            get { return (string)base[_networkName]; }
        }

        [ConfigurationProperty("botNickname", IsRequired = true)]
        public string BotNickname
        {
            get { return (string)base[_botNickname]; }
        }

        [ConfigurationProperty("botRealName", IsRequired = true)]
        public string BotRealName
        {
            get { return (string)base[_botRealName]; }
        }

        [ConfigurationProperty("botUserName", IsRequired = true)]
        public string BotUserName
        {
            get { return (string)base[_botUserName]; }
        }

        [ConfigurationProperty("nickServPassword", IsRequired = true)]
        public string NickServPassword
        {
            get { return (string)base[_nickServPassword]; }
        }

        [ConfigurationProperty("servers", IsRequired = true)]
        public ServerElementCollection Servers
        {
            get { return (ServerElementCollection)base[_servers]; }
        }

        [ConfigurationProperty("channels", IsRequired = true)]
        public ChannelElementCollection Channels
        {
            get { return (ChannelElementCollection)base[_channels]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        #region Constructors

        static NetworkElement()
        {
            _networkName = new ConfigurationProperty("networkName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _botNickname = new ConfigurationProperty("botNickname", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _botRealName = new ConfigurationProperty("botRealName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _botUserName = new ConfigurationProperty("botUserName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _nickServPassword = new ConfigurationProperty("nickServPassword", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _servers = new ConfigurationProperty("servers", typeof(ServerElementCollection), null, ConfigurationPropertyOptions.IsRequired);
            _channels = new ConfigurationProperty("channels", typeof(ChannelElementCollection), null, ConfigurationPropertyOptions.IsRequired);

            _properties = new ConfigurationPropertyCollection();

            _properties.Add(_networkName);
            _properties.Add(_botNickname);
            _properties.Add(_botRealName);
            _properties.Add(_botUserName);
            _properties.Add(_nickServPassword);
            _properties.Add(_servers);
            _properties.Add(_channels);

        }
        #endregion
    }
}