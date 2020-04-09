using System;
using System.Globalization;
using TimeZoneConverter;

namespace PostDietProgress.Extensions
{
    public static class ParseExtensions
    {
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

            if (!parseResult)
            {
                var dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateJst, jstTimeZone);

                return dateUtc;
            }

            return null;
        }
    }
}
