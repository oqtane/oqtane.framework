using System;
using System.Globalization;
using System.Reflection;

namespace Oqtane.Infrastructure
{
    public class RightToLeftCulture
    {
        public static CultureInfo ResolveFormat(CultureInfo cultureInfo)
        {
            SetNumberFormatInfo(cultureInfo.NumberFormat);
            SetCalenar(cultureInfo);

            return cultureInfo;
        }

        private static void SetCalenar(CultureInfo cultureInfo)
        {
            var calendar = new RightToLeftCultureCalendar();

            var fieldInfo = cultureInfo.GetType().GetField("_calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(cultureInfo, calendar);
            }

            var info = cultureInfo.DateTimeFormat.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                info.SetValue(cultureInfo.DateTimeFormat, calendar);
            }
        }

        public static void SetNumberFormatInfo(NumberFormatInfo persianNumberFormatInfo)
        {
            persianNumberFormatInfo.NumberDecimalSeparator = ".";
            persianNumberFormatInfo.DigitSubstitution = DigitShapes.NativeNational;
            persianNumberFormatInfo.NumberNegativePattern = 0;
            persianNumberFormatInfo.NegativeSign = "-";
        }
    }
}
