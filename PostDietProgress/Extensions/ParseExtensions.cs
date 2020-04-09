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
    }
}
