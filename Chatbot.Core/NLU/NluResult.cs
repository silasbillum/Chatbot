using System.Collections.Generic;

namespace Chatbot.Core.NLU
{
    // Extended NluResult to support RawResponse for AI fallback transparency
    public record NluResult(string Intent, Dictionary<string, string> Entities, string RawResponse = null);
}
