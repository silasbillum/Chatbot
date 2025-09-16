   
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
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
    public async Task<NluResult> AnalyzeAsync(string userInput)
    {
        // Systemprompt for at tvinge dansk
        string systemPrompt = "Du er en hjælper, der altid svarer på flydende dansk. " +
                              "Svar aldrig på svensk eller norsk, og brug kun dansk i alle svar.";

        var request = new
        {
            model = "mistral",    // eller "phi3:mini", "mistral"
            prompt = $"{systemPrompt}\n\nBrugerens input: {userInput}"
        };

        // Stream response from Ollama so the answer can be shown as it is being written
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _http.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new System.IO.StreamReader(stream);
        string? line;
        var answerBuilder = new System.Text.StringBuilder();
        while ((line = await reader.ReadLineAsync()) != null)
        {
            answerBuilder.AppendLine(line);
            // Optionally: Here you could fire an event/callback to update the UI in real time
        }
        var answer = answerBuilder.ToString().Trim();

        // Try to parse as JSON, else treat as plain text
        string intent = "ExternalFallback";
        var entities = new Dictionary<string, string>();
        bool parsed = false;
        try
        {
            using var doc = JsonDocument.Parse(answer);
            if (doc.RootElement.TryGetProperty("intent", out var intentProp))
            {
                intent = intentProp.GetString() ?? intent;
                parsed = true;
            }
            if (doc.RootElement.TryGetProperty("entities", out var entitiesProp) && entitiesProp.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in entitiesProp.EnumerateObject())
                {
                    entities[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }
            }
        }
        catch
        {
            // Not JSON, treat as plain text answer
        }

        if (!parsed)
        {
            // If not JSON, try to extract a clean answer (remove quotes, newlines, etc.)
            intent = "ai_answer";
            var clean = answer;
            if ((clean.StartsWith("\"") && clean.EndsWith("\"")) || (clean.StartsWith("'") && clean.EndsWith("'")))
            {
                clean = clean.Substring(1, clean.Length - 2);
            }
            clean = clean.Replace("\n", " ").Replace("\r", " ").Trim();
            entities["answer"] = clean;
        }

        // Set RawResponse for transparency
        return new NluResult(
            intent,
            entities,
            answer
        );
    }

             // Stream AI response line by line for real-time UI updates
    public async IAsyncEnumerable<string> StreamAiAnswerAsync(string userInput)
    {
        string systemPrompt = "Du er en hjælper, der altid svarer på flydende dansk. Svar aldrig på svensk eller norsk, og brug kun dansk i alle svar.";
        var request = new
        {
            model = "mistral",
            prompt = $"{systemPrompt}\n\nBrugerens input: {userInput}"
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _http.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new System.IO.StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            yield return line + " ";
        }
    }
}
