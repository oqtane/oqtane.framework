using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ILocalizationService
    {
        Task<IEnumerable<Culture>> GetCulturesAsync();
    }
}
