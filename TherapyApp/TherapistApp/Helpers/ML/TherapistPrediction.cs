using Microsoft.ML.Data;

namespace TherapyApp.Helpers.ML;

public class TherapistPrediction
{
    [ColumnName("PredictedLabel")]
    public uint Cluster { get; set; } 
}