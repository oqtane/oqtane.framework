using System;
using System.Collections.Generic;
using System.Text;
using Oqtane.Models;

namespace Oqtane.Providers
{
    public interface IFolderProviderFactory
    {
        IFolderProvider GetProvider(int folderConfigId);

        int GetDefaultConfigId(int siteId);
    }
}
