using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TherapyApp.Entities;
using TherapyApp.Helpers.Dto;

namespace TherapyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiaryController : ControllerBase
    {
        private readonly TherapyDbContext _db;
        public DiaryController(TherapyDbContext db)
        {
            _db = db;
        }

        // POST: api/Diary/create
        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateDiary()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid token");
                }

                var newDiary = new Diary()
                {
                    UserId = userId,
                    User = await _db.Users.Where(u => u.Id == userId).SingleOrDefaultAsync()
                };

                await _db.Diaries.AddAsync(newDiary);
                await _db.SaveChangesAsync();

                return Ok(newDiary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Diary/add
        [Authorize]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddDiaryEntry([FromBody] DiaryEntryDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                //Get token from header
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                //Decode token
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                //Get userid
                var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

                var diary = await _db.Diaries.Where(d => d.UserId == userId).FirstOrDefaultAsync();

                if (diary == null) {
                    return BadRequest("Diary doesn't exist");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid token");
                }

                var emote = await _db.Emotions.Where(e => e.Id == model.EmotionId).FirstOrDefaultAsync();
                if (emote == null)
                {
                    return BadRequest("Emotion doesn't exist");
                }

                var newEntry = new DiaryEntry()
                {
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow,
                    DiaryId = diary.Id,
                    Diary = diary,
                    EmotionId = model.EmotionId,
                    Emotion = emote,    
                };

                await _db.DiaryEntries.AddAsync(newEntry);
                await _db.SaveChangesAsync();

                return Ok(newEntry);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        // GET: api/Diary/entries/id
        [HttpGet]
        [Route("entries/{id}")]
        public async Task<IActionResult> GetAllDiaryEntries(int id)
        {
            var entriesList = await _db.DiaryEntries.Where(e => e.DiaryId == id).ToListAsync();

            return Ok(entriesList);
        }
    }
}
