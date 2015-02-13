using System;
using System.Globalization;

namespace SmartDb4.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static string ToMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }

        public static string ToShortMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        public static int GetWeek(this DateTime dateTime)
        {
            const CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;

            const DayOfWeek firstWeekDay = DayOfWeek.Monday;

            var calendar = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;

            int currentWeek = calendar.GetWeekOfYear(dateTime, weekRule, firstWeekDay);

            return currentWeek;
        }

        public static int GetFutureWeek(this DateTime dateTime, int futureWeekCount)
        {
            DateTime myFutureWeek = DateTime.Now.AddDays(futureWeekCount * 7); // add N weeks

            const CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;

            const DayOfWeek firstWeekDay = DayOfWeek.Monday;

            var calendar = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;

            int futureWeek = calendar.GetWeekOfYear(myFutureWeek, weekRule, firstWeekDay);

            return futureWeek;
        }

        //public static DateTime FirstDateOfWeek(this DateTime dateTime, int year, int weekNum, CalendarWeekRule rule)
        //{
        //    var jan1 = new DateTime(year, 1, 1);

        //    int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
        //    var firstMonday = jan1.AddDays(daysOffset);

        //    var cal = CultureInfo.CurrentCulture.Calendar;
        //    int firstWeek = cal.GetWeekOfYear(firstMonday, rule, DayOfWeek.Monday);

        //    if (firstWeek <= 1)
        //    {
        //        weekNum -= 1;
        //    }

        //    var result = firstMonday.AddDays(weekNum * 7);

        //    return result;
        //}
        public static DateTime FirstDateOfWeek(this DateTime dateTime, int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
    }
}