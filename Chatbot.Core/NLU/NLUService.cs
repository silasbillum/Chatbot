using System;
using System.IO;
using Chatbot.Core.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualBasic;

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

        public List<List<string>> UnsupervisedIntentExtraction(string csvPath, int numClusters = 4)
        {
            if (!File.Exists(csvPath)) throw new FileNotFoundException(csvPath);

            var unclassifiedSentences = File.ReadAllLines(csvPath)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Split(',')[0].Trim()) // Only take the sentence, not the label
                .ToList();

            if (unclassifiedSentences.Count < 2)
                throw new ArgumentException("No unclassified sentences available for unsupervised intent extraction.");

            var dataView = _mlContext.Data.LoadFromEnumerable(unclassifiedSentences.Select(s => new ChatInput { Text = s }));
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(ChatInput.Text))
                .Append(_mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: numClusters));

            var model = pipeline.Fit(dataView);
            var transformedData = model.Transform(dataView);
            var predictions = _mlContext.Data.CreateEnumerable<ClusterPrediction>(transformedData, reuseRowObject: false).ToList();

            var clusters = new List<List<string>>();
            for (int i = 0; i < numClusters; i++)
                clusters.Add(new List<string>());

            for (int i = 0; i < predictions.Count; i++)
                clusters[(int)predictions[i].PredictedClusterId - 1].Add(unclassifiedSentences[i]);

            return clusters;
        }




        public NluResult Predict(string text)
        {
            if (_predictor == null)
                throw new InvalidOperationException("Model not trained.");

            var prediction = _predictor.Predict(new ChatInput { Text = text });

            // Sæt en threshold, fx 0.5
            if (prediction.Score.Max() < 0.5)
                return new NluResult("unknown", new Dictionary<string, string>(), null);

            // Ekstra check: intent-ordet skal optræde tidligt i sætningen
            if (string.IsNullOrWhiteSpace(prediction.PredictedIntent))
                return new NluResult("unknown", new Dictionary<string, string>(), null);

            var intentWord = prediction.PredictedIntent.ToLower();
            var textLower = text.ToLower();
            int idx = textLower.IndexOf(intentWord);
            // Hvis intent-ordet ikke findes, eller først efter 40% af sætningen, returner unknown
            if (idx == -1 || idx > textLower.Length * 0.4)
                return new NluResult("unknown", new Dictionary<string, string>(), null);

            // Only allow known intents, otherwise fallback to AI
            var knownIntents = new HashSet<string> { "add_to_cart", "ask_price", "checkout", "confirm", "goodbye" };
            if (!knownIntents.Contains(prediction.PredictedIntent))
                return new NluResult("unknown", new Dictionary<string, string>(), null);

            return new NluResult(prediction.PredictedIntent, new Dictionary<string, string>(), null);
        }

        public void Dispose()
        {
            // nothing to dispose for now
        }
    }
}
