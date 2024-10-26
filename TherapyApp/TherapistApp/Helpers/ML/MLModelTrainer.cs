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
        IDataView trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(PatientTherapistTraining.EmotionalStates))
                        .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(PatientTherapistTraining.TherapistSpecializationId)))
                        .Append(_mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated("Label", "Features"))
                        .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        var model = pipeline.Fit(trainingDataView);

        _mlContext.Model.Save(model, trainingDataView.Schema, modelPath);
    }
}