using System.Linq;

namespace EtaskMinstry.AppCode
{
    /// <summary>
    /// Task estimated time (Task.ExpectedTime + Task.TimeUnitID).
    ///
    /// Agreed units: Day = 3, Hour = 4, Minute = 5, and 1 Day = 8 Hours = 480 Minutes -
    /// the same conversion dbo.GetCountMiniuts / dbo.GetWorkMinutes use for the activity
    /// level, so a 1-day task and an 8-hour task count the same.
    /// An estimate is always saved as a pair: a positive time with one of those units.
    /// </summary>
    public static class TaskTimeEstimate
    {
        public const int MinutesPerHour = 60;
        public const int HoursPerDay = 8;
        public const int MinutesPerDay = HoursPerDay * MinutesPerHour;
        private const int DaysPerWeek = 5;
        private const int WeeksPerMonth = 4;

        public static readonly int[] AllowedUnits = { (int)TimeUnit.Day, (int)TimeUnit.Hour, (int)TimeUnit.Minute };

        public static bool IsAllowedUnit(int? unit)
        {
            return unit.HasValue && AllowedUnits.Contains(unit.Value);
        }

        /// <summary>The estimate in minutes, or null for no estimate / an unknown unit.</summary>
        public static decimal? ToMinutes(decimal? count, int? unit)
        {
            if (!count.HasValue || !unit.HasValue)
                return null;
            switch (unit.Value)
            {
                case (int)TimeUnit.Month: return count.Value * MinutesPerDay * DaysPerWeek * WeeksPerMonth;
                case (int)TimeUnit.Week: return count.Value * MinutesPerDay * DaysPerWeek;
                case (int)TimeUnit.Day: return count.Value * MinutesPerDay;
                case (int)TimeUnit.Hour: return count.Value * MinutesPerHour;
                case (int)TimeUnit.Minute: return count.Value;
                default: return null;
            }
        }

        /// <summary>No estimate at all, or a positive time with an allowed unit.</summary>
        public static bool IsValid(decimal? expected, int? unit)
        {
            if (!expected.HasValue || expected.Value <= 0)
                return true;
            return IsAllowedUnit(unit);
        }

        /// <summary>A unit without a time is not an estimate: drop it so the pair stays consistent.</summary>
        public static int? UnitToSave(decimal? expected, int? unit)
        {
            return expected.HasValue && expected.Value > 0 ? unit : null;
        }

        /// <summary>
        /// Estimates saved before the agreed units used Month/Week: show them as the same
        /// number of days so editing the task keeps the value (1 week = 5 days, 1 month = 20 days).
        /// </summary>
        public static void ToAllowedUnit(ref decimal? expected, ref int? unit)
        {
            if (!expected.HasValue || IsAllowedUnit(unit) || !unit.HasValue)
                return;
            var minutes = ToMinutes(expected, unit);
            if (!minutes.HasValue)
                return;
            expected = minutes.Value / MinutesPerDay;
            unit = (int)TimeUnit.Day;
        }

        /// <summary>
        /// The estimate fits in the task's working days (8 hours each). Replaces the old
        /// per-unit checks that allowed 24 hours a day.
        /// </summary>
        public static bool FitsWorkingDays(decimal count, int unit, decimal workingDays)
        {
            if (!IsAllowedUnit(unit))
                return false;
            return ToMinutes(count, unit).Value <= workingDays * MinutesPerDay;
        }
    }
}
