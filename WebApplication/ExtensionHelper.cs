namespace WebApplication
{
    public static class ExtensionHelper
    {
        public static string AsNullIfEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str) ? str : null;
        }
    }
}