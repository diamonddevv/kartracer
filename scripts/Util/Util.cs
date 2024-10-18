using System;

public static partial class Util
{
    public static string GetOrEmptyFallback(this string s, string fallback)
    {
        if (s == null || s.Length == 0)
        {
            return fallback;
        }
        else return s;
    }

    public static string Limit(this string s, int length)
    {
        return s.Substring(0, Math.Min(s.Length, length));
    }

    public static bool IsBitSet(uint i, int idx_1indexed)
    {
        return (i & (1 << (idx_1indexed - 1))) != 0;
    }
}
