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
    [ConfigurationCollection(typeof(LicenseElement), AddItemName="license", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class LicenseElementCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties;

        #region Properties

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "license"; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        #endregion

        static LicenseElementCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        public LicenseElementCollection() { }

        #region Indexers

        public LicenseElement this[int index]
        {
            get { return (LicenseElement)base.BaseGet(index); }

            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }

                base.BaseAdd(index, value);
            }
        }

        public new LicenseElement this[string name]
        {
            get { return (LicenseElement)base.BaseGet(name); }
        }

        #endregion

        protected override ConfigurationElement CreateNewElement()
        {
            return new LicenseElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as LicenseElement).ApplicationName;
        }
    }
}