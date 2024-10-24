using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using NuGet.DependencyResolver;
using TherapyApp.Entities;
using TherapyApp.Helpers.ML;
using TherapyApp.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TherapyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly TherapyDbContext _db;
        private readonly MLModelTrainer _mLModelTrainer;
        private readonly MLModelPredictor _mPredictor;
        private readonly ICsvService _csvService;
        public RecommendationController(TherapyDbContext db, MLModelTrainer mLModelTrainer, MLModelPredictor mPredictor, ICsvService csvService)
        {
            _db = db;
            _mLModelTrainer = mLModelTrainer;
            _mPredictor = mPredictor;
            _csvService = csvService;
        }

        // GET: api/<RecommendationController>
        [HttpPost("Save")]
        public IActionResult SaveTrainingData()
        {
            try
            {
                var trainingData = _csvService.GetTrainingData();
                _csvService.SaveTrainingDataToCsv(trainingData, "training_data.csv");
                return Ok("Saved Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while saving the training data: {ex.Message}");
            }
        }

        // GET: api/<RecommendationController>
        [HttpPost("Train")]
        public IActionResult TrainModel()
        {
            try
            {
                var trainingData = _csvService.GetTrainingData();
                _mLModelTrainer.TrainModel(trainingData, "therapist_model.zip");
                return Ok("Trained Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while training the model: {ex.Message}");
            }
        }

        // POST api/<RecommendationController>
        [HttpPost("Predict")]
        public IActionResult PredictSpec()
        {
            try
            {
                var emotionalStateData = new float[] { 0.9f, 0.9f, 0.7f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
                var result = _mPredictor.Predict("therapist_model.zip", emotionalStateData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while predicting the spec: {ex.Message}");
            }
        }
    }
}

