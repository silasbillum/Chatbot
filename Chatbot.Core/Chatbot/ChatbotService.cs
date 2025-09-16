      

using Chatbot.Core.DM;
using Chatbot.Core.Entities;
using Chatbot.Core.NLG;
using Chatbot.Core.NLU;
using System;
using System.Collections.Generic;

namespace Chatbot.Core.Chatbot
{
    public class ChatbotService : IDisposable
    {
        private readonly INluEngine _nlu;
        private readonly INlgEngine _nlg;
        private readonly DialogManager _dialog;

        public ChatbotService(string trainingCsvPath)
        {
            var mlNetEngine = new NLUService(trainingCsvPath);
            var httpClient = new System.Net.Http.HttpClient();
            var aiEngine = new AiNluEngine(httpClient);
            _nlu = new HybridNluEngine(mlNetEngine, aiEngine);
            _nlg = new NLGService();
            _dialog = new DialogManager();
        }

        public async Task<string> HandleMessage(string sessionId, string message)
        {
            NluResult result;
            // Try async if available
            if (_nlu is HybridNluEngine hybrid)
                result = await hybrid.PredictAsync(message);
            else
                result = _nlu.Predict(message);
            var intent = result.Intent;

            if (intent == "unknown")
            {
                File.AppendAllText(@"c:\Users\Silas\Desktop\Folders\C#\Chatbot\Chatbot.Web\Data\unclassifiedSentences.csv", $"{message},unknown{Environment.NewLine}");
            }

            if (message.IndexOf("add", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("buy", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                intent = "add_to_cart";
            }
            if (message.IndexOf("price", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                intent = "ask_price";
            }
            if (message.IndexOf("confirm", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                intent = "confirm";
            }

            var entities = EntityExtractor.Extract(message);

            var response = _dialog.Handle(sessionId, intent, entities, _nlg);
            return response;
        }

        public List<List<string>> ClusterUnclassifiedSentences(string csvPath, int numClusters = 3)
        {

            return ((NLUService)_nlu).UnsupervisedIntentExtraction(csvPath, numClusters);
        }

        public void SaveUnclassifiedSentence(string sentence, string csvPath)
        {
            if (!string.IsNullOrWhiteSpace(sentence))
                File.AppendAllText(csvPath, sentence + Environment.NewLine);
        }

        // Brug AiNluEngine.AnalyzeAsync til robust AI-svar
        public async Task<string> GetAiTextAsync(string userInput)
        {
            if (_nlu is HybridNluEngine hybrid)
            {
                var aiEngineField = typeof(HybridNluEngine).GetField("_aiEngine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var aiEngine = aiEngineField?.GetValue(hybrid) as AiNluEngine;
                if (aiEngine != null)
                {
                    var nluResult = await aiEngine.AnalyzeAsync(userInput);
                    var answer = nluResult.Entities != null && nluResult.Entities.TryGetValue("answer", out var a) ? a : null;
                    if (string.IsNullOrWhiteSpace(answer))
                        return "(AI svar mangler)";
                    // Del op i sætninger og saml pænt
                    var sentences = answer
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                        .Split(new[] {'.', '!', '?'}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s));
                    return string.Join(". ", sentences) + (answer.EndsWith(".") ? "." : "");
                }
            }
            return "(AI svar mangler)";
        }

        public void Dispose()
        {
            _nlu?.Dispose();
        }

    }
}
