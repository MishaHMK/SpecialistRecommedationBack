using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.LightGbm;
using TherapyApp.Entities;

namespace TherapyApp.Helpers.ML;

public class MLModelTrainer
{
    private readonly MLContext _mlContext;

    public MLModelTrainer()
    {
        _mlContext = new MLContext();
    }

    public void TrainModel(List<PatientTherapistTraining> trainingData, string modelPath)
    {
        // Load training data
        IDataView trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Define the pipeline
        var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(PatientTherapistTraining.EmotionalStates))
                        .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(PatientTherapistTraining.TherapistSpecializationId)))
                        .Append(_mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated("Label", "Features"))
                        .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // Train the model
        Console.WriteLine("Training the model...");
        var model = pipeline.Fit(trainingDataView);
        Console.WriteLine("Model training complete!");

        // Save the trained model
        _mlContext.Model.Save(model, trainingDataView.Schema, modelPath);
        Console.WriteLine($"Model saved at {modelPath}");
    }


    public void EvaluateModel(string csvFilePath, string modelPath)
    {
        var testDataView = _mlContext.Data.LoadFromTextFile<PatientTherapistData>(
            csvFilePath,
            hasHeader: true,
            separatorChar: ',');

        ITransformer trainedModel = _mlContext.Model.Load(modelPath, out var modelInputSchema);

        var predictions = trainedModel.Transform(testDataView);

        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

        Console.WriteLine($"Log-loss: {metrics.LogLoss}");
        Console.WriteLine($"MicroAccuracy: {metrics.MicroAccuracy}");
        Console.WriteLine($"MacroAccuracy: {metrics.MacroAccuracy}");
    }

}