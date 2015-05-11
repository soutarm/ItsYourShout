using System;
using System.Globalization;

namespace ItsYourShout.Classes
{
    public static class Utility
    {
        /// <summary>
        /// Returns a string with the years, weeks, days, hours and minutes that have passed between the input date and "now" UTC.
        /// </summary>
        /// <param name="utcDateToCompare">DateTime: A UTC date which should be before now.</param>
        /// <returns>string: a formatted string containing the time difference between UTC Now and the UTC input date.</returns>
        public static string GetTimeDifference(this DateTime utcDateToCompare)
        {
            const string plural = "s";
            const string minutesFormat = "{0} minute{1}";
            const string hoursFormat = "{0} hour{1}";
            const string daysFormat = "{0} day{1}";
            const string weeksFormat = "{0} week{1}";
            const string yearsFormat = "{0} year{1}";
            const string twoValueFormat = "{0} and {1} ago";
            const string standardFormat = "{0} ago";
            const string rightNow = "just now";

            const int daysInAYear = 365;
            const int weeksInAYear = 52;
            const int daysInAWeek = 7;

            var yearsOld = System.DateTime.Now.Subtract(utcDateToCompare).Days / daysInAYear;
            var yearsString = string.Format(yearsFormat, yearsOld, yearsOld == 1 ? string.Empty : plural);

            var weeksOld = (System.DateTime.Now.Subtract(utcDateToCompare).Days / daysInAWeek) - (yearsOld * weeksInAYear);
            var weeksString = string.Format(weeksFormat, weeksOld, weeksOld == 1 ? string.Empty : plural);

            var daysOld = System.DateTime.Now.Subtract(utcDateToCompare).Days - (weeksOld * daysInAWeek);
            var daysString = string.Format(daysFormat, daysOld, daysOld == 1 ? string.Empty : plural);

            var hoursOld = System.DateTime.Now.Subtract(utcDateToCompare).Hours;
            var hoursString = string.Format(hoursFormat, hoursOld, hoursOld == 1 ? string.Empty : plural);

            var minutesOld = System.DateTime.Now.Subtract(utcDateToCompare).Minutes;
            var minutesString = string.Format(minutesFormat, minutesOld, minutesOld == 1 ? string.Empty : plural);

            if (yearsOld > 2)
            {
                return string.Format(standardFormat, yearsString);
            }
            if (yearsOld > 0)
            {
                return weeksOld < 1 ? string.Format(standardFormat, yearsString) : string.Format(twoValueFormat, yearsString, weeksString);
            }
            if (weeksOld > 4)
            {
                return string.Format(standardFormat, weeksString);
            }
            if (weeksOld > 0)
            {
                return daysOld < 1 ? string.Format(standardFormat, weeksString) : string.Format(twoValueFormat, weeksString, daysString);
            }
            if (daysOld > 0)
            {
                return hoursOld < 1 ? string.Format(standardFormat, daysString) : string.Format(twoValueFormat, daysString, hoursString);
            }
            if (hoursOld > 0)
            {
                return minutesOld < 1 ? string.Format(standardFormat, hoursString) : string.Format(twoValueFormat, hoursString, minutesString);
            }

            // Final condition, if not met use the default in case something was published in the "future".
            return minutesOld > 0 ? string.Format(standardFormat, minutesString) : rightNow;

        }

        public static string CacheBreaker()
        {
            return System.DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
        }

    }
}
