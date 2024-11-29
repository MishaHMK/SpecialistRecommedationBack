using Microsoft.ML;
using TherapyApp.Entities;
using TherapyApp.Services;

namespace TherapyApp.Helpers.ML;

public class MLModelTrainer
{
    private readonly MLContext _mlContext;
    private readonly ICsvService _csvService;

    public MLModelTrainer(ICsvService csvService)
    {
        _mlContext = new MLContext();
        _csvService = csvService;
    }

    public void TrainModel(List<PatientTherapistTraining> trainingData, string modelPath)
    {
        IDataView trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms
            .Concatenate("Features", nameof(PatientTherapistTraining.EmotionalStates))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(PatientTherapistTraining.TherapistSpecializationId)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var model = pipeline.Fit(trainingDataView);

        _mlContext.Model.Save(model, trainingDataView.Schema, modelPath);
    }

    public void TrainModelFromCsv(string csvFilePath, string modelFilePath)
    {
        var trainingData = _csvService.LoadTrainingDataFromCsv(csvFilePath);
        TrainModel(trainingData, modelFilePath);
    }
}