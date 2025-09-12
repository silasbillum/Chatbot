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
        private readonly NLUService _nlu;
        private readonly NLGService _nlg;
        private readonly DialogManager _dialog;

        public ChatbotService(string trainingCsvPath)
        {
            _nlu = new NLUService(trainingCsvPath);
            _nlg = new NLGService();
            _dialog = new DialogManager();
        }

        public string HandleMessage(string sessionId, string message)
        {
            var (intent, scores) = _nlu.Predict(message);

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

        

        public void Dispose()
        {
            _nlu?.Dispose();
        }
    }
}
