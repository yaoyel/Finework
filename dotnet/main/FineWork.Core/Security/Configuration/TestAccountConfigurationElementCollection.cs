using System;
using System.Configuration;

namespace FineWork.Security.Configuration
{
    public class TestAccountConfigurationElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TestAccountConfigurationElement();
        }

        protected override ConfigurationElement CreateNewElement(String elementName)
        {
            return new TestAccountConfigurationElement(elementName);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            return ((TestAccountConfigurationElement) element).Name;
        }

        public TestAccountConfigurationElement Get(String name)
        {
            return (TestAccountConfigurationElement)BaseGet(name);
        }
    }
}