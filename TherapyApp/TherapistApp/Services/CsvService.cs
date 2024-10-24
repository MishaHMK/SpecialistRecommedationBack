using System.IO;
using Newtonsoft.Json;
using TherapyApp.Entities;

namespace TherapyApp.Services
{
    public class CsvService : ICsvService
    {
        public void SaveTrainingDataToCsv(List<PatientTherapistTraining> trainingData, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // Write CSV header
                writer.WriteLine(
                    "Anxiety,Depression,Addiction,Panic,PTSD,Anger,Phobia,Delirium,Fatigue,FunctionalDisorder,TherapistSpecializationId");

                // Write each row of data
                foreach (var data in trainingData)
                {
                    // Deserialize the JSON string back to float[] (emotional states)
                    var emotionalStates = data.EmotionalStates;

                    // Create a line with emotional states followed by TherapistSpecializationId
                    var line = string.Join(",", emotionalStates) + $",{data.TherapistSpecializationId}";
                    writer.WriteLine(line);
                }
            }
        }

        public List<PatientTherapistTraining> GetTrainingData()
        {
            return new List<PatientTherapistTraining>
        {
        // Addiction Treatment (5 cases)
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.9f, 0.5f, 0.8f, 0.3f, 0.2f, 0.4f, 0.1f, 0.0f, 0.1f, 0.2f },
                TherapistSpecializationId = 1
            },
            new PatientTherapistTraining
            {
                EmotionalStates =  new float[] { 0.85f, 0.55f, 0.75f, 0.25f, 0.15f, 0.35f, 0.2f, 0.0f, 0.2f, 0.1f },
                TherapistSpecializationId = 1
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.8f, 0.6f, 0.9f, 0.35f, 0.25f, 0.3f, 0.3f, 0.1f, 0.3f, 0.2f },
                TherapistSpecializationId = 1
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.75f, 0.45f, 0.85f, 0.2f, 0.2f, 0.4f, 0.1f, 0.0f, 0.15f, 0.05f },
                TherapistSpecializationId = 1
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.7f, 0.4f, 0.9f, 0.3f, 0.25f, 0.35f, 0.25f, 0.05f, 0.2f, 0.15f },
                TherapistSpecializationId = 1
            },

        // Trauma Therapy (5 cases)
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.7f, 0.9f, 0.2f, 0.8f, 0.9f, 0.8f, 0.6f, 0.2f, 0.1f, 0.15f },
                TherapistSpecializationId = 2
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.65f, 0.85f, 0.25f, 0.75f, 0.85f, 0.75f, 0.65f, 0.25f, 0.1f, 0.1f },
                TherapistSpecializationId = 2
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.6f, 0.8f, 0.3f, 0.7f, 0.9f, 0.7f, 0.55f, 0.3f, 0.15f, 0.2f },
                TherapistSpecializationId = 2
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.75f, 0.9f, 0.35f, 0.85f, 0.85f, 0.85f, 0.7f, 0.3f, 0.2f, 0.25f },
                TherapistSpecializationId = 2
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.8f, 0.75f, 0.4f, 0.75f, 0.8f, 0.75f, 0.65f, 0.4f, 0.25f, 0.3f },
                TherapistSpecializationId = 2
            },

            // Child Therapy (5 cases)
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.2f, 0.3f, 0.2f, 0.6f, 0.7f, 0.5f, 0.8f, 0.7f, 0.6f, 0.4f },
                TherapistSpecializationId = 3
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.25f, 0.4f, 0.15f, 0.55f, 0.65f, 0.6f, 0.75f, 0.6f, 0.55f, 0.45f },
                TherapistSpecializationId = 3
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.3f, 0.35f, 0.25f, 0.5f, 0.6f, 0.55f, 0.7f, 0.65f, 0.55f, 0.4f },
                TherapistSpecializationId = 3
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.35f, 0.45f, 0.1f, 0.45f, 0.55f, 0.6f, 0.7f, 0.55f, 0.6f, 0.35f },
                TherapistSpecializationId = 3
            },
            new PatientTherapistTraining
            {
                EmotionalStates =  new float[] { 0.4f, 0.5f, 0.3f, 0.4f, 0.5f, 0.55f, 0.65f, 0.6f, 0.55f, 0.35f },
                TherapistSpecializationId = 3
            },

            // Family Therapy (5 cases)
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.4f, 0.5f, 0.4f, 0.3f, 0.6f, 0.65f, 0.55f, 0.4f, 0.45f, 0.55f },
                TherapistSpecializationId = 4
            },
            new PatientTherapistTraining
            {
                EmotionalStates =  new float[] { 0.35f, 0.6f, 0.45f, 0.35f, 0.7f, 0.55f, 0.5f, 0.45f, 0.55f, 0.6f },
                TherapistSpecializationId = 4
            },
            new PatientTherapistTraining
            {
                EmotionalStates =  new float[] { 0.3f, 0.4f, 0.35f, 0.4f, 0.65f, 0.6f, 0.6f, 0.5f, 0.5f, 0.65f },
                TherapistSpecializationId = 4
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.45f, 0.55f, 0.5f, 0.45f, 0.5f, 0.55f, 0.6f, 0.55f, 0.4f, 0.6f },
                TherapistSpecializationId = 4
            },
            new PatientTherapistTraining
            {
                EmotionalStates =  new float[] { 0.5f, 0.6f, 0.55f, 0.5f, 0.55f, 0.6f, 0.65f, 0.6f, 0.5f, 0.65f },
                TherapistSpecializationId = 4
            },

            // Counseling (5 cases)
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.25f, 0.3f, 0.25f, 0.35f, 0.4f, 0.45f, 0.55f, 0.5f, 0.45f, 0.6f },
                TherapistSpecializationId = 5
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.3f, 0.4f, 0.3f, 0.4f, 0.45f, 0.5f, 0.6f, 0.55f, 0.4f, 0.55f },
                TherapistSpecializationId = 5
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.35f, 0.35f, 0.35f, 0.45f, 0.5f, 0.55f, 0.65f, 0.55f, 0.45f, 0.5f },
                TherapistSpecializationId = 5
            },
            new PatientTherapistTraining
            {
                EmotionalStates = new float[] { 0.4f, 0.45f, 0.4f, 0.5f, 0.55f, 0.6f, 0.65f, 0.6f, 0.5f, 0.55f },
                TherapistSpecializationId = 5
            },
            new PatientTherapistTraining
            {
                EmotionalStates =  new float[] { 0.45f, 0.5f, 0.45f, 0.55f, 0.6f, 0.65f, 0.7f, 0.65f, 0.55f, 0.6f },
                TherapistSpecializationId = 5
            }
        };
        }
    }
}
