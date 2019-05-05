using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISkinRepository
    {
        IEnumerable<Skin> GetSkins();
    }
}
