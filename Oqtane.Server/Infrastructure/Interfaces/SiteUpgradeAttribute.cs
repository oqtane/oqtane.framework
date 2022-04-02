using System;

namespace Oqtane.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SiteUpgradeAttribute : Attribute
    {
        private string aliasname;
        private string version;

        public SiteUpgradeAttribute(string AliasName, string Version)
        {
            aliasname = AliasName;
            version = Version;
        }

        public virtual string AliasName
        {
            get { return aliasname; }
        }

        public virtual string Version
        {
            get { return version; }
        }
    }
}
