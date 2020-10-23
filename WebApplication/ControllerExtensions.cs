namespace Helpers
{
    public static class ControllerExtensions
    {
        public static string ShortControllerName(this string fullControllerClassName)
        {
            return fullControllerClassName.Replace("Controller", string.Empty);
        }
    }
}