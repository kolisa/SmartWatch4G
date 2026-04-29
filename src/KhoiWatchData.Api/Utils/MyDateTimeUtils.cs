using System.Globalization;

namespace KhoiWatchData.Api.Utils
{
    public static class MyDateTimeUtils
    {
        public static string ParsePbDateTime(long seconds)
        {
            try
            {
                // Convert the seconds to DateTime
                System.DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

                // Format the DateTime to the desired string format
                string dateStr = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                return dateStr;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool IsValidDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return false;

            return System.DateTime.TryParseExact(
                dateStr,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        public static string GetPreviousDay(string dateStr)
        {
            if (!IsValidDate(dateStr)) return string.Empty;

            var date = System.DateTime.ParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return date.AddDays(-1).ToString("yyyy-MM-dd");
        }
    }
}
