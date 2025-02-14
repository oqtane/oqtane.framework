using System;
using System.Globalization;
using System.Reflection;

namespace Oqtane.Infrastructure
{
    public class PersianCulture
    {
        public static CultureInfo GetPersianCultureInfo()
        {
            var persianCultureInfo = new CultureInfo("fa-IR");

            SetPersianDateTimeFormatInfo(persianCultureInfo.DateTimeFormat);
            SetNumberFormatInfo(persianCultureInfo.NumberFormat);

            var cal = new PersianCalendar();

            FieldInfo fieldInfo = persianCultureInfo.GetType().GetField("_calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(persianCultureInfo, cal);
            }

            FieldInfo info = persianCultureInfo.DateTimeFormat.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                info.SetValue(persianCultureInfo.DateTimeFormat, cal);
            }

            return persianCultureInfo;
        }

        public static void SetPersianDateTimeFormatInfo(DateTimeFormatInfo persianDateTimeFormatInfo)
        {
            persianDateTimeFormatInfo.MonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", string.Empty };
            persianDateTimeFormatInfo.MonthGenitiveNames = persianDateTimeFormatInfo.MonthNames;
            persianDateTimeFormatInfo.AbbreviatedMonthNames = persianDateTimeFormatInfo.MonthNames;
            persianDateTimeFormatInfo.AbbreviatedMonthGenitiveNames = persianDateTimeFormatInfo.MonthNames;

            persianDateTimeFormatInfo.DayNames = new[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
            persianDateTimeFormatInfo.AbbreviatedDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            persianDateTimeFormatInfo.ShortestDayNames = persianDateTimeFormatInfo.AbbreviatedDayNames;
            persianDateTimeFormatInfo.FirstDayOfWeek = DayOfWeek.Saturday;

            persianDateTimeFormatInfo.AMDesignator = "ق.ظ";
            persianDateTimeFormatInfo.PMDesignator = "ب.ظ";

            persianDateTimeFormatInfo.DateSeparator = "/";
            persianDateTimeFormatInfo.TimeSeparator = ":";

            persianDateTimeFormatInfo.FullDateTimePattern = "tt hh:mm:ss yyyy mmmm dd dddd";
            persianDateTimeFormatInfo.YearMonthPattern = "yyyy, MMMM";
            persianDateTimeFormatInfo.MonthDayPattern = "dd MMMM";

            persianDateTimeFormatInfo.LongDatePattern = "dddd, dd MMMM,yyyy";
            persianDateTimeFormatInfo.ShortDatePattern = "yyyy/MM/dd";

            persianDateTimeFormatInfo.LongTimePattern = "hh:mm:ss tt";
            persianDateTimeFormatInfo.ShortTimePattern = "hh:mm tt";
        }

        public static void SetNumberFormatInfo(NumberFormatInfo persianNumberFormatInfo)
        {
            persianNumberFormatInfo.NumberDecimalSeparator = "/";
            persianNumberFormatInfo.DigitSubstitution = DigitShapes.NativeNational;
            persianNumberFormatInfo.NumberNegativePattern = 0;
            persianNumberFormatInfo.NegativeSign = "-";
        }
    }
}
