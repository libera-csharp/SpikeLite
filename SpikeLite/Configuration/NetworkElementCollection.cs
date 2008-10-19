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
    [ConfigurationCollection(typeof(NetworkElement), AddItemName="network", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class NetworkElementCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;

        #region Properties

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "network"; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        static NetworkElementCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        #region Indexers

        public NetworkElement this[int index]
        {
            get { return (NetworkElement)BaseGet(index); }

            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                base.BaseAdd(index, value);
            }
        }

        public new NetworkElement this[string name]
        {
            get { return (NetworkElement)BaseGet(name); }
        }

        #endregion

        protected override ConfigurationElement CreateNewElement()
        {
            return new NetworkElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as NetworkElement).NetworkName;
        }
    }
}