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
    [ConfigurationCollection(typeof(ServerElement), AddItemName="server", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ServerElementCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties;

        #region Properties

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "server"; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        static ServerElementCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        public ServerElementCollection() {  }

        #region Indexers

        public ServerElement this[int index]
        {
            get { return (ServerElement)base.BaseGet(index); }

            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }

                base.BaseAdd(index, value);
            }
        }

        public new ServerElement this[string name]
        {
            get { return (ServerElement)base.BaseGet(name); }
        }

        #endregion

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            ServerElement serverElement = element as ServerElement;

            return string.Format("{0}:{1}",serverElement.HostnameOrIP, serverElement.Port);
        }
    }
}