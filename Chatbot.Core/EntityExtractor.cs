using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Chatbot.Core
{
    public static class EntityExtractor
    {
        private static readonly string[] KnownProducts = new[] { "bread", "milk", "apple", "banana", "eggs", "cheese", "coffee" };

        public static Dictionary<string, string> Extract(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var m = Regex.Match(text, "\\b(\\d+)\\b");
            if (m.Success) result["number"] = m.Groups[1].Value;

            foreach (var p in KnownProducts)
            {
                if (Regex.IsMatch(text, $"\\b{Regex.Escape(p)}\\b", RegexOptions.IgnoreCase))
                {
                    result["product"] = p;
                    break;
                }
            }

            if (Regex.IsMatch(text, "\\b(yes|yep|sure|yeah|ok)\\b", RegexOptions.IgnoreCase)) result["affirm"] = "true";
            if (Regex.IsMatch(text, "\\b(no|nah|not)\\b", RegexOptions.IgnoreCase)) result["deny"] = "true";

            return result;
        }
    }
}
