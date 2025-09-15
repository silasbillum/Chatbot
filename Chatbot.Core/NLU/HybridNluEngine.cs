using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chatbot.Core.Models;
using Chatbot.Core.NLU;

namespace Chatbot.Core.NLU
{
    public class HybridNluEngine : INluEngine, IDisposable
    {
        private readonly NLUService _mlNetEngine;
        private readonly AiNluEngine _aiEngine;

        public HybridNluEngine(NLUService mlNetEngine, AiNluEngine aiEngine)
        {
            _mlNetEngine = mlNetEngine;
            _aiEngine = aiEngine;
        }

        public NluResult Predict(string text)
        {
            var result = _mlNetEngine.Predict(text);
            if (result.Intent == "unknown")
            {
                // Fallback til ekstern AI (synkron wrapper)
                var aiTask = _aiEngine.AnalyzeAsync(text);
                aiTask.Wait();
                return aiTask.Result;
            }
            return result;
        }

        public async Task<NluResult> PredictAsync(string text)
        {
            var result = _mlNetEngine.Predict(text);
            if (result.Intent == "unknown")
            {
                return await _aiEngine.AnalyzeAsync(text);
            }
            return result;
        }

        public void Reset()
        {
            // Hvis ML.NET engine har en reset-metode, kald den her
            // Hvis AiNluEngine har en reset-metode, kald den her
            // Ellers gør intet
        }

        public void Dispose()
        {
            _mlNetEngine?.Dispose();
            // AiNluEngine har typisk ikke noget at dispose
        }
    }
}
