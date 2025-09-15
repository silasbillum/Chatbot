using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Chatbot.Core.Entities
{
    public static class EntityExtractor
    {

        public static class ProductCatalog
        {
            public static HashSet<string> Products { get; private set; }

            static ProductCatalog()
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "products.csv");
                Products = File.ReadAllLines(path).Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim().ToLower()).ToHashSet();
            }
        }


        public static class TrainingDataCatalog
        {
            public static List<string> TrainingData { get; private set; }

            static TrainingDataCatalog()
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "trainingData.csv");
                TrainingData = File.ReadAllLines(path)
                    .Skip(1)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim())
                    .ToList();
            }
        }

        public static class unclassifiedSentencesCatalog
        {
            public static List<string> unclassifiedSentences { get; private set; }

            static unclassifiedSentencesCatalog()
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "unclassifiedSentences.csv");
                unclassifiedSentences = File.ReadAllLines(path)
                    .Skip(1)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim())
                    .ToList();
            }
        }


        public static Dictionary<string, string> Extract(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var m = Regex.Match(text, "\\b(\\d+)\\b");
            if (m.Success) result["number"] = m.Groups[1].Value;

            foreach (var p in ProductCatalog.Products)
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
