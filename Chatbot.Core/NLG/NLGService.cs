using System;
using System.Collections.Generic;

namespace Chatbot.Core.NLG
{
    public class NLGService
    {
        public string Generate(string intent, Dictionary<string,string> entities)
        {
            entities ??= new Dictionary<string,string>();

            return intent switch
            {
                "greeting" => "Hello! Welcome to our supermarket. How can I help you today?",
                "ask_products" => "We have bread, milk, fruits, vegetables, and household items. Anything specific?",
                "ask_price" => entities.TryGetValue("product", out var p) ? $"The price for {p} is $2.99 (sample)." : "Which product do you mean?",
                "add_to_cart" => entities.TryGetValue("product", out var pr) && entities.TryGetValue("number", out var qty)
                                    ? $"Added {qty} x {pr} to your cart."
                                    : entities.TryGetValue("product", out var pr2) ? $"Added {pr2} to your cart." : "What would you like to add?",
                "checkout" => "Sure — proceeding to checkout. Your total is $10.00 (sample). Would you like to confirm?",
                "confirm" => "Order confirmed. Thank you for shopping with us!",
                "goodbye" => "Goodbye! Have a great day.",
                _ => "Sorry, I didn't understand. Could you rephrase?"
            };
        }
    }
}
