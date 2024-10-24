using Microsoft.ML.Data;
using System.ComponentModel.DataAnnotations;

namespace TherapyApp.Entities;

public class PatientTherapistData
{
    [Key] 
    public int Id { get; set; }
    [VectorType(10)]
    public string EmotionalStates { get; set; } = string.Empty;
    public int TherapistSpecializationId { get; set; }
}

