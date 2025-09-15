using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Chatbot.Core.Models;
using Chatbot.Core.NLU;

namespace Chatbot.Core.NLU;

public class AiNluEngine
{
    private readonly HttpClient _http;

    public AiNluEngine(HttpClient http)
    {
        _http = http;
    }
    /*public async Task<NluResult> AnalyzeAsync(string userInput)
    {
        // Systemprompt for at tvinge dansk
        string systemPrompt = "Du er en hjælper, der altid svarer på flydende dansk. " +
                              "Svar aldrig på svensk eller norsk, og brug kun dansk i alle svar.";

        var request = new
        {
            model = "mistral",    // eller "phi3:mini", "mistral"
            prompt = $"{systemPrompt}\n\nBrugerens input: {userInput}"
        };

        var response = await _http.PostAsJsonAsync("http://localhost:11434/api/generate", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return new NluResult
        {
            Intent = "ExternalFallback",
            Entities = new Dictionary<string, string>(),
            RawResponse = json
        };
    }*/

}
