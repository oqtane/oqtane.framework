using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository.Interfaces
{
    public interface IThemeRepository
    {
        IEnumerable<Theme> GetThemes();
    }
}
