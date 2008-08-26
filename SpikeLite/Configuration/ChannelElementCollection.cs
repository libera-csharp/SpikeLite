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
    [ConfigurationCollection(typeof(ChannelElement), AddItemName="channel", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ChannelElementCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties;

        #region Properties

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "channel"; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        static ChannelElementCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        public ChannelElementCollection() { }

        #region Indexers

        public ChannelElement this[int index]
        {
            get { return (ChannelElement)base.BaseGet(index); }

            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new ChannelElement this[string name]
        {
            get { return (ChannelElement)base.BaseGet(name); }
        }

        #endregion

        protected override ConfigurationElement CreateNewElement()
        {
            return new ChannelElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ChannelElement).Name;
        }
    }
}