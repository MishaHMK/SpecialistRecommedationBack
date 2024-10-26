using Microsoft.ML;
using Microsoft.ML.Data;
using TherapyApp.Entities;

namespace TherapyApp.Helpers.ML;

public class MLModelPredictor
{
    private readonly MLContext _mlContext;

    public MLModelPredictor()
    {
        _mlContext = new MLContext();
    }

    public float Predict(string modelPath, float[] emotionalStateData)
    {
        var loadedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);

        var predictionEngine = _mlContext.Model
            .CreatePredictionEngine<PatientTherapistTraining, TherapistPrediction>(loadedModel);

        var input = new PatientTherapistTraining { EmotionalStates = emotionalStateData };

        var prediction = predictionEngine.Predict(input);

        return prediction.TherapistSpecializationId;
    }

    public class TherapistPrediction
    {
        [ColumnName("PredictedLabel")]
        public float TherapistSpecializationId { get; set; }
    }
}
