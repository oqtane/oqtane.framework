using System;
using System.Collections.Generic;

namespace Oqtane.Themes
{
    public interface ITheme
    {
        Dictionary<string, string> Properties { get; }
    }
}
