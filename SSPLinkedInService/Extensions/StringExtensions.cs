namespace SSPLinkedInService.Extensions
{
    public static class StringExtensions
    {
        public static int AsInt32(this string value, int baseValue = 0)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }

            return baseValue;
        }
    }
}
