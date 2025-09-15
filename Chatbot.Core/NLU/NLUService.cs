using System;
using System.IO;
using Chatbot.Core.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Chatbot.Core.NLU
{
    public class NLUService : IDisposable, INluEngine
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<ChatInput, ChatPrediction> _predictor;
        private ITransformer _model;

        public NLUService(string trainingDataPath)
        {
            _mlContext = new MLContext(seed: 0);
            Train(trainingDataPath);
        }

        public void Train(string csvPath)
        {
            if (!File.Exists(csvPath)) throw new FileNotFoundException(csvPath);

            var dataView = _mlContext.Data.LoadFromTextFile<ChatInput>(csvPath, hasHeader: true, separatorChar: ',');
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(ChatInput.Text))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ChatInput.Intent)))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            _model = pipeline.Fit(dataView);
            _predictor = _mlContext.Model.CreatePredictionEngine<ChatInput, ChatPrediction>(_model);
        }

        public NluResult Predict(string text)
        {
            if (_predictor == null) throw new InvalidOperationException("Model not trained.");
            var pred = _predictor.Predict(new ChatInput { Text = text });
            // Entity extraction logic can be added here. For now, return empty dictionary.
            var entities = new Dictionary<string, string>();
            return new NluResult(pred.PredictedIntent, entities);
        }

        public void Dispose()
        {
            // nothing to dispose for now
        }
    }
}
