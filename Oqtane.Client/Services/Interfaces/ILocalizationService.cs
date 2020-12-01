using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ILocalizationService
    {
        Task<string> GetDefaultCulture();

        Task<string[]> GetSupportedCultures();
    }
}
