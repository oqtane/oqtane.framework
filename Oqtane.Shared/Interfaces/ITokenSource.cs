using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oqtane.Interfaces
{
    public interface ITokenSource
    {
        IDictionary<string, object> GetTokens();
    }
}
