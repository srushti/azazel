using System;

namespace Azazel.Extensions {
    public static class StaticString {
        public static bool ContainsIgnoreCase(this string fullString, string potentialSubstring) {
            return fullString.IndexOf(potentialSubstring, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        public static string Substring(this string fullString, string startString, string endString) {
            var startIndex = fullString.IndexOf(startString);
            var endIndex = fullString.IndexOf(endString);
            if (endIndex == -1) return fullString.Substring(startIndex);
            var length = endIndex + endString.Length - startIndex;
            return fullString.Substring(startIndex, length);
        }

        public static string AllButLastCharacter(this string s)
        {
            if (s.Length == 0) return s;
            return s.Substring(0, s.Length - 1);
        }
    }
}