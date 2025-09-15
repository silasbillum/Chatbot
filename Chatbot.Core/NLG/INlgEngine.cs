
using Chatbot.Core.NLU;
using System;
namespace Chatbot.Core.NLG
{
    public interface INlgEngine
    {
        string GenerateResponse(string intent, Dictionary<string, string> entities);
    }
}