
namespace Chatbot.Core.NLU
{
    // Moved to NluResult.cs for RawResponse support

    public interface INluEngine
    {
        void Train(string csvPath);
        NluResult Predict(string text);
        void Dispose();
    }
}