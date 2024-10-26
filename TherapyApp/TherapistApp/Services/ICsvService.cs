using TherapyApp.Entities;

namespace TherapyApp.Services;

public interface ICsvService
{
    public void SaveTrainingDataToCsv(List<PatientTherapistTraining> trainingData, string filePath);
    public List<PatientTherapistTraining> LoadTrainingDataFromCsv(string filePath);
    public List<PatientTherapistTraining> GetTrainingData();
}
