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

    public string Predict(string modelPath, float[] emotionalStateData)
    {
        // Load the trained model
        var loadedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);

        // Create a prediction engine
        var predictionEngine = _mlContext.Model
            .CreatePredictionEngine<PatientTherapistTraining, TherapistPrediction>(loadedModel);

        // Prepare the input
        var input = new PatientTherapistTraining { EmotionalStates = emotionalStateData };

        // Make a prediction
        var prediction = predictionEngine.Predict(input);

        return $"Predicted Therapist Specialization: {prediction.TherapistSpecializationId}";
    }

    public class TherapistPrediction
    {
        [ColumnName("PredictedLabel")]
        public float TherapistSpecializationId { get; set; }
    }
}
