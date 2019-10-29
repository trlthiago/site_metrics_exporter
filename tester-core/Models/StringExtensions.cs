namespace tester_core.Models
{
    public static class StringExtensions
    {
        public static string NormalizeUrl(this string url)
        {
            if (url.StartsWith("http:") || url.StartsWith("https:"))
                return url;
            return "http://" + url;
        }
    }
}
