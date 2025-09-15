
namespace Chatbot.Core.NLU
{
    public record NluResult(string Intent, Dictionary<string, string> Entities);

    public interface INluEngine
    {
        void Train(string csvPath);
        NluResult Predict(string text);
        void Dispose();
    }
}