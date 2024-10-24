using Microsoft.ML.Data;
using System.ComponentModel.DataAnnotations;

namespace TherapyApp.Entities;

public class PatientTherapistTraining
{
    [VectorType(10)]
    public float[] EmotionalStates { get; set; } = new float[10];
    public float TherapistSpecializationId { get; set; }
}

