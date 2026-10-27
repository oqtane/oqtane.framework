using System;
using System.Collections.Generic;
using System.Text;
using Oqtane.Models;

namespace Oqtane.Providers
{
    public interface IStorageProviderFactory
    {
        IStorageProvider GetProvider(Folder folder);

        int GetDefaultConfigId(int siteId);

        int GetStorageLocationId(int siteId, string provider);
    }
}
