using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TherapyApp.Entities;

namespace TherapyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmotionsController : ControllerBase
    {
        private readonly TherapyDbContext _db;
        public EmotionsController(TherapyDbContext db)
        {
            _db = db;
        }

        // POST: api/Emotion/add
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> CreateEmotion([FromBody] Emotion model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Emotion newEmotion = new Emotion()
            {
                Name = model.Name,
                ImageUrl = model.ImageUrl
            };

            await _db.Emotions.AddAsync(newEmotion);
            await _db.SaveChangesAsync();

            return Ok(newEmotion);
        }

        // GET: api/Emotion/id
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetEmotionById(int id)
        {
            var emotion = await _db.Emotions.Where(x => x.Id == id).SingleOrDefaultAsync();

            return Ok(emotion);
        }


        // GET: api/Emotion
        [HttpGet]
        public async Task<IActionResult> GetAllEmotions()
        {
            var emotionList = await _db.Emotions.ToListAsync();

            return Ok(emotionList);
        }

    }
}
