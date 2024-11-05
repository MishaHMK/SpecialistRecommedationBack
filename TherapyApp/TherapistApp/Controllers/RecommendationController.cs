using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI_API.Completions;
using OpenAI_API;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TherapyApp.Helpers.ML;
using TherapyApp.Services;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Globalization;

namespace TherapyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly TherapyDbContext _db;
        private readonly MLModelTrainer _mLModelTrainer;
        private readonly MLModelPredictor _mPredictor;
        private readonly ChatGptService _chatGptService;
        private readonly ICsvService _csvService;
        public RecommendationController(TherapyDbContext db, MLModelTrainer mLModelTrainer,
            MLModelPredictor mPredictor, ICsvService csvService, ChatGptService chatGptService)
        {
            _db = db;
            _mLModelTrainer = mLModelTrainer;
            _mPredictor = mPredictor;
            _csvService = csvService;
            _chatGptService = chatGptService;
        }

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

        [HttpGet("Train")]
        public IActionResult TrainModel()
        {
            try
            {
                var trainingData = _csvService.LoadTrainingDataFromCsv("training_data.csv");
                _mLModelTrainer.TrainModel(trainingData, "therapist_model.zip");
                return Ok("Trained Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while training the model: {ex.Message}");
            }
        }

        [HttpGet("Predict")]
        public IActionResult PredictSpec()
        {
            try
            {
                var emotionalStateData = new float[] { 0.65f, 0.8f, 0.2f, 0.75f, 0.9f, 0.7f, 0.55f, 0.3f, 0.15f, 0.2f };
                var result = _mPredictor.Predict("therapist_model.zip", emotionalStateData);
                return Ok($"Predicted Therapist Specialization: {result}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while predicting the spec: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("Recommendation")]
        public async Task<IActionResult> GetRecommendation()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

                var diary = await _db.Diaries.Where(d => d.UserId == userId).FirstOrDefaultAsync();

                if (diary == null)
                {
                    return BadRequest("Diary doesn't exist");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid token");
                }

                var entries = await _db.DiaryEntries
                    .Where(e => e.DiaryId == diary.Id && e.CreatedAt >= DateTime.Now.AddDays(-10))
                    .Include(e => e.Emotion)
                    .ToListAsync();

                var emotionAverages = new float[10]; 
                var groupedEntries = entries.GroupBy(e => e.EmotionId);

                foreach (var group in groupedEntries)
                {
                    var emotionId = group.Key - 1; 
                    emotionAverages[emotionId] = (float)group.Average(e => e.Value); 
                }

                var specId = _mPredictor.Predict("therapist_model.zip", emotionAverages);
                var res = _db.TherapistUsers.Where(tu => tu.SpecialityId == specId)
                    .Include(x => x.User)
                    .Include(x => x.Speciality);

                using (var writer = new StreamWriter("training_data.csv", append: true))
                {
                    var emotionalStates = emotionAverages
                           .Select(v => v.ToString(CultureInfo.InvariantCulture))
                           .ToArray();

                    var line = string.Join(",", emotionalStates) + $",{specId}";
                    writer.WriteLine(line);

                }

                return Ok(res);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpGet("GetAnswer")]
        public async Task<IActionResult> GetAnswer()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
                var diary = await _db.Diaries.Where(d => d.UserId == userId).FirstOrDefaultAsync();

                if (diary == null)
                {
                    return BadRequest("Diary doesn't exist");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid token");
                }

                var emotions = await _db.DiaryEntries
                    .Where(e => e.DiaryId == diary.Id && 
                                   e.CreatedAt >= DateTime.Now.AddDays(-10))
                    .Include(e => e.Emotion)
                    .OrderByDescending(e => e.Value)
                    .ToListAsync();


                var distinctEmotionNames = emotions
                    .GroupBy(e => e.EmotionId)
                    .Select(g => g.First().Emotion.Name)
                    .Take(3)
                    .ToList();

                if (distinctEmotionNames == null)
                {
                    return BadRequest("No recent diary entries");
                }

                var promptVar = String.Empty;

                foreach (var e in distinctEmotionNames)
                {
                    promptVar += $" {e}";
                }

                var prompt = $"Which basic, brief 5 tips would you recommend " +
                    $"with my mental conditions: {promptVar}?";

                var answer = await _chatGptService.GetConciseAnswer(prompt);
                return Ok(answer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}

