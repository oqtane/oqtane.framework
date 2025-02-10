using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oqtane.Shared
{
    public sealed class SharedConverter
    {
        public static int ParseInteger(string value)
        {
            return ParseInteger(value, CultureInfo.InvariantCulture);
        }

        public static int ParseInteger(string value, CultureInfo cultureInfo)
        {
            return ParseInteger(value, cultureInfo);
        }
    }
}
