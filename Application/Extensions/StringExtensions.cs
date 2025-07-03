using System;
using System.Globalization;

namespace Application.Extensions
{
    public static class StringExtensions
    {
        public static string CapitalizeFirst(this string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s ?? "";
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}