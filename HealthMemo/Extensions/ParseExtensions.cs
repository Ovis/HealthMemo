using System;
using System.Globalization;
using TimeZoneConverter;

namespace HealthMemo.Extensions
{
    public static class ParseExtensions
    {
        private static readonly DateTime UnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public static double? ToDoubleOrNull(this string str)
        {
            if (double.TryParse(str, out var result))
            {
                return result;
            }
            return null;
        }

        public static long? ToLongOrNull(this string str)
        {
            if (long.TryParse(str, out var result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// JSTな日時文字列をUTCな日時オブジェクトに変換
        /// </summary>
        /// <param name="str"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime? TryJstDateTimeStringParseToUtc(this string str, string format = "yyyyMMddHHmm")
        {
            var jstCulture = new CultureInfo("ja-JP");
            var jstTimeZone = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");

            var parseResult = DateTime.TryParseExact(str,
                format,
                jstCulture,
                DateTimeStyles.NoCurrentDateDefault, out var dateJst);

            if (parseResult)
            {
                var dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateJst, jstTimeZone);

                return dateUtc;
            }

            return null;
        }

        /// <summary>
        /// UTCな日時文字列をJSTな日時オブジェクトに変換
        /// </summary>
        /// <param name="str"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime? TryUtcDateTimeStringParseToJst(this string str, string format = "yyyyMMddHHmm")
        {
            var jstTimeZone = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");

            var parseResult = DateTime.TryParseExact(str,
                format,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.NoCurrentDateDefault, out var dateUtc);

            if (parseResult)
            {
                var dateJst = TimeZoneInfo.ConvertTimeFromUtc(dateUtc, jstTimeZone);

                return dateJst;
            }

            return null;
        }

        public static DateTime? ConvertJstTimeFromUtc(this DateTime? utcDate)
        {
            var jstTimeZone = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");

            if (utcDate == null) return null;

            var dateJst = TimeZoneInfo.ConvertTimeFromUtc((DateTime)utcDate, jstTimeZone);

            return dateJst;

        }

        public static DateTime ConvertJstTimeFromUtc(this DateTime utcDate)
        {
            var jstTimeZone = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");

            var dateJst = TimeZoneInfo.ConvertTimeFromUtc((DateTime)utcDate, jstTimeZone);

            return dateJst;

        }

        /// <summary>
        /// UNIX時間でのナノ秒値
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetUnixEpochNanoseconds(DateTime dt)
        {
            return (dt.Ticks - UnixEpochStart.Ticks) * 100;
        }

    }
}
