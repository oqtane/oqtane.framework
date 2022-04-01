using Oqtane.Models;
using Oqtane.Documentation;

namespace Oqtane.Infrastructure
{
    [PrivateApi("Mark Site-Template classes as private, since it's not very useful in the public docs")]
    public class ExampleUpgrade : IUpgradeable
    {
        string IUpgradeable.GetVersions(Alias alias)
        {
            var versions = "";
            switch (alias.Name)
            {
                case "localhost:44357":
                    // return the list of official release versions for the specific site
                    versions = "1.0.0";
                    break;
            }
            return versions;
        }

        bool IUpgradeable.Upgrade(Alias alias, string version)
        {
            bool success = true;
            switch (alias.Name)
            {
                case "localhost:44357":
                    switch (version)
                    {
                        case "1.0.0":
                            // execute some version-specific upgrade logic for the site here such as adding pages, modules, content, etc...
                            success = true;
                            break;
                    }
                    break;
            }
            return success;
        }
    }
}
