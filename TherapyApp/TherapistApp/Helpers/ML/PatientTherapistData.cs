using Microsoft.ML.Data;

namespace TherapyApp.Helpers.ML;

public class PatientTherapistData
{
    [LoadColumn(0)] public float Anxiety { get; set; }
    [LoadColumn(1)] public float Depression { get; set; }
    [LoadColumn(2)] public float Addiction { get; set; }
    [LoadColumn(3)] public float Panic { get; set; }
    [LoadColumn(4)] public float PTSD { get; set; }
    [LoadColumn(5)] public float Anger { get; set; }
    [LoadColumn(6)] public float Phobia { get; set; }
    [LoadColumn(7)] public float Delirium { get; set; }
    [LoadColumn(8)] public float Fatigue { get; set; }
    [LoadColumn(9)] public float FunctionalDisorder { get; set; }

    [LoadColumn(10)] public int TherapistSpecializationId { get; set; } 
}
