using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISkinService
    {
        Task<List<Skin>> GetSkinsAsync();
    }
}
