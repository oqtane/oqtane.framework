using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IThemeRepository
    {
        IEnumerable<Theme> GetThemes();
    }
}
