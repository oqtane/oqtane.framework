using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISystemService
    {
        Task<Dictionary<string, string>> GetSystemInfoAsync();
    }
}
