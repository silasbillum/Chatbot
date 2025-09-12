using Microsoft.ML.Data;

namespace Chatbot.Core
{
    public class ChatInput
    {
        [LoadColumn(0)]
        public string Text { get; set; }

        [LoadColumn(1)]
        public string Intent { get; set; }
    }

    public class ChatPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedIntent { get; set; }

        public float[] Score { get; set; }
    }
}
