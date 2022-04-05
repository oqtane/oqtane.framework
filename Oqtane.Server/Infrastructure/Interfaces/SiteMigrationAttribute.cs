using System;

namespace Oqtane.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SiteMigrationAttribute : Attribute
    {
        private string aliasname;
        private string version;

        public SiteMigrationAttribute(string AliasName, string Version)
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
