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

        public string HandleMessage(string sessionId, string message)
        {
            var result = _nlu.Predict(message);
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

        public void Dispose()
        {
            _nlu?.Dispose();
        }
    }
}
