using System;

namespace Utils
{
    public static class StringExtensionMethods
    {
        public static string AfterLast(this string str, string sub)
        {
            int idx = str.LastIndexOf(sub, StringComparison.Ordinal);
            return idx < 0 ? "" : str.Substring(idx + sub.Length);
        }

        public static string BeforeLast(this string str, string sub)
        {
            int idx = str.LastIndexOf(sub, StringComparison.Ordinal);
            return idx < 0 ? "" : str.Substring(0, idx);
        }

        public static string AfterFirst(this string str, string sub)
        {
            int idx = str.IndexOf(sub, StringComparison.Ordinal);
            return idx < 0 ? "" : str.Substring(idx + sub.Length);
        }

        public static string BeforeFirst(this string str, string sub)
        {
            int idx = str.IndexOf(sub, StringComparison.Ordinal);
            return idx < 0 ? "" : str.Substring(0, idx);
        }
    }
}