﻿using Microsoft.AspNetCore.Authorization;
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
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

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
                    Value = model.Value
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


        // GET: api/Diary/entries/id?page=1&pageSize=10
        [Authorize]
        [HttpGet]
        [Route("entries")]
        public async Task<IActionResult> GetAllDiaryEntries(int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            var diaryId = await _db.Diaries.Where(d => d.UserId == userId).Select(d => d.Id).FirstOrDefaultAsync();

            var query = _db.DiaryEntries
                           .Where(e => e.DiaryId == diaryId)
                           .Include(x => x.Emotion);

            var totalCount = await query.CountAsync();

            var entriesList = await query
                                .OrderByDescending(e => e.CreatedAt)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var result = new
            {
                TotalCount = totalCount,
                Entries = entriesList
            };

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("patients")]
        public async Task<IActionResult> GetTherapistPatients(int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Get the therapist's user ID from the token
            var therapistUserId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var therapistRole = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

            if (string.IsNullOrEmpty(therapistUserId))
            {
                return Unauthorized("Invalid token.");
            }

            // Check if the user is a therapist
            var isTherapist = await _db.Users.AnyAsync(u => u.Id == therapistUserId && therapistRole == "Therapist");
            if (!isTherapist)
            {
                return Forbid("You are not authorized to access this resource.");
            }

            // Fetch distinct patients from meetings (past and future)
            var patientsQuery = _db.Meetings
                .Where(m => m.TherapistId == therapistUserId)
                .Select(m => m.Client)
                .Distinct();

            var totalPatients = await patientsQuery.CountAsync();

            var patients = await patientsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.FirstName,
                    p.LastName,
                    p.Email,
                    p.PhoneNumber
                })
                .ToListAsync();

            var result = new
            {
                TotalCount = totalPatients,
                Patients = patients
            };

            return Ok(result);
        }

        // GET: api/Diary/patient_entries/id?page=1&pageSize=10
        [Authorize]
        [HttpGet]
        [Route("patient_entries")]
        public async Task<IActionResult> GetAllPatientEntries(string userId, int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and pageSize must be greater than 0.");
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var therapistUserId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
            var therapistRole = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;
            var isTherapist = await _db.Users.AnyAsync(u => u.Id == therapistUserId && therapistRole == "Therapist");

            if (!isTherapist)
            {
                return Forbid("You are not authorized to access this resource.");
            }


            var diaryId = await _db.Diaries.Where(d => d.UserId == userId).Select(d => d.Id).FirstOrDefaultAsync();

            var query = _db.DiaryEntries
                           .Where(e => e.DiaryId == diaryId)
                           .Include(x => x.Emotion);

            var totalCount = await query.CountAsync();

            var entriesList = await query
                                .OrderByDescending(e => e.CreatedAt)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var result = new
            {
                TotalCount = totalCount,
                Entries = entriesList
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("mydiary")]
        public async Task<IActionResult> GetMyDiaryId()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            //Get userid
            var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            var diaryId = await _db.Diaries
                                    .Where(d => d.UserId == userId)
                                    .Select(x => x.Id)
                                    .FirstOrDefaultAsync();

            return Ok(diaryId);
        }


        [HttpGet]
        [Route("existdiary")]
        public async Task<IActionResult> IfDiaryExists()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            var diary= await _db.Diaries
                                    .Where(d => d.UserId == userId)
                                    .FirstOrDefaultAsync();

            if (diary == null)
            {
                return Ok(false);
            }

            return Ok(true);
        }


        [Authorize]
        [HttpDelete]
        [Route("entryremove/{id}")]
        public async Task<IActionResult> DeleteEntryById(int id)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jsonToken?.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            var entryToDelete = await _db.DiaryEntries.Where(d => d.Id == id)
                                                     .FirstOrDefaultAsync();
            if (entryToDelete == null)
            {
                throw new NotImplementedException();
            }

            var diary = await _db.Diaries.Where(d => d.Id == entryToDelete.DiaryId)
                                                     .FirstOrDefaultAsync();

            if (diary == null || diary.UserId != userId)
            {
                throw new NotSupportedException();
            }

            _db.DiaryEntries.Remove(entryToDelete);
            _db.SaveChanges();

            return Ok();
        }
    }
}
