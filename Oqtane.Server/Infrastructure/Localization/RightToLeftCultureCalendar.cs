using System;

namespace Oqtane.Infrastructure
{
    public class RightToLeftCultureCalendar : System.Globalization.PersianCalendar
    {
        public override int GetYear(DateTime time)
        {
            try
            {
                return base.GetYear(time);
            }
            catch
            {
                // ignore
            }

            return time.Year;
        }

        public override int GetMonth(DateTime time)
        {
            try
            {
                return base.GetMonth(time);
            }
            catch
            {
                // ignore
            }

            return time.Month;
        }

        public override int GetDayOfMonth(DateTime time)
        {
            try
            {
                return base.GetDayOfMonth(time);
            }
            catch
            {
                // ignore
            }

            return time.Day;
        }

        public override int GetDayOfYear(DateTime time)
        {
            try
            {
                return base.GetDayOfYear(time);
            }
            catch
            {
                // ignore
            }

            return time.DayOfYear;
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            try
            {
                return base.GetDayOfWeek(time);
            }
            catch
            {
                // ignore
            }

            return time.DayOfWeek;
        }
    }
}
