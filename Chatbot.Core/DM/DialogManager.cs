using Chatbot.Core.NLG;
using System;
using System.Collections.Generic;

namespace Chatbot.Core.DM
{
    public class DialogState
    {
        public List<string> Cart { get; set; } = new List<string>();
        public bool AwaitingConfirmation { get; set; } = false;
    }

    public class DialogManager
    {
        private readonly Dictionary<string, DialogState> _conversations = new();

        private DialogState GetState(string sessionId)
        {
            if (!_conversations.TryGetValue(sessionId, out var st))
            {
                st = new DialogState();
                _conversations[sessionId] = st;
            }
            return st;
        }

        public string Handle(string sessionId, string intent, Dictionary<string, string> entities, INlgEngine nlg)
        {
            var state = GetState(sessionId);

            switch (intent)
            {
                case "add_to_cart":
                    if (entities != null && entities.TryGetValue("product", out var p))
                    {
                        var qty = entities.ContainsKey("number") ? entities["number"] : "1";
                        state.Cart.Add($"{qty}x {p}");
                        return nlg.GenerateResponse("add_to_cart", entities);
                    }
                    return nlg.GenerateResponse("add_to_cart", entities);

                case "checkout":
                    state.AwaitingConfirmation = true;
                    return nlg.GenerateResponse("checkout", entities);

                case "confirm":
                    if (state.AwaitingConfirmation)
                    {
                        state.Cart.Clear();
                        state.AwaitingConfirmation = false;
                        return nlg.GenerateResponse("confirm", entities);
                    }
                    return "There's nothing to confirm.";

                case "goodbye":
                    return nlg.GenerateResponse("goodbye", entities);

                default:
                    return nlg.GenerateResponse(intent, entities);
            }
        }
    }
}
