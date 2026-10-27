using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Oqtane.Providers
{
    public interface IStorageProviderSettingsControl
    {
        int StorageLocationId { get; set; }

        Task<IDictionary<string, string>> GetSettingsAsync();
    }
}
