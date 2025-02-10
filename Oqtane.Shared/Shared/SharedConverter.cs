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
            return ParseInteger(value, CultureInfo.InvariantCulture, 0);
        }

        public static int ParseInteger(string value, CultureInfo cultureInfo)
        {
            return ParseInteger(value, cultureInfo, 0);
        }

        public static int ParseInteger(string value, CultureInfo cultureInfo, int defaultValue)
        {
            if (int.TryParse(value, cultureInfo, out int result))
            {
                return result;
            }

            return 0;
        }
    }
}
