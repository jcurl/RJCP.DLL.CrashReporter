namespace RJCP.Diagnostics.Config
{
    using System;
    using System.Configuration;

    internal sealed class WatchdogOverrides : ConfigurationElementCollection
    {
        private const string ItemName = "Task";

        public new WatchdogOverride this[string name]
        {
            get { return (WatchdogOverride)BaseGet(name); }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return ItemName; }
        }

        protected override bool IsElementName(string elementName)
        {
            return ItemName.Equals(elementName, StringComparison.Ordinal);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WatchdogOverride)element).Task;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WatchdogOverride();
        }
    }
}
