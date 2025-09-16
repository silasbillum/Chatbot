   
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
        var rawBuilder = new System.Text.StringBuilder();
        while ((line = await reader.ReadLineAsync()) != null)
        {
            rawBuilder.AppendLine(line);
            try
            {
                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.TryGetProperty("response", out var respProp))
                {
                    var chunk = respProp.GetString();
                    if (!string.IsNullOrEmpty(chunk))
                        answerBuilder.Append(chunk);
                }
            }
            catch
            {
                // Not JSON, ignore
            }
        }
        var answer = answerBuilder.ToString();
        var raw = rawBuilder.ToString();
        // Fjern alle linjeskift og trim mellemrum
        answer = answer.Replace("\r", " ").Replace("\n", " ").Trim();
        Console.WriteLine("[OLLAMA RAW RESPONSE]");
        Console.WriteLine(raw);
        Console.WriteLine($"[OLLAMA AI ANSWER LENGTH]: {answer.Length}");
        Console.WriteLine($"[OLLAMA AI ANSWER]: '{answer}'");

        var entities = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(answer))
            entities["answer"] = answer;
        else
            entities["answer"] = "";
        return new NluResult(
            "ai_answer",
            entities,
            raw
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
