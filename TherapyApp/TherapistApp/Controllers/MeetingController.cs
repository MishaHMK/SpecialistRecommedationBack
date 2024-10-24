using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TherapyApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TherapyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
        private readonly TherapyDbContext _db;
        public MeetingController(TherapyDbContext db)
        {
            _db = db;
        }

        //// POST: api/Meeting/create
        //[Authorize]
        //[HttpPost]
        //[Route("create")]
        //public async Task<IActionResult> CreateMeeting([FromBody] Meeting model, int therapistId)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    try
        //    {
        //        //Get token from header
        //        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        //        //Decode token
        //        var handler = new JwtSecurityTokenHandler();
        //        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        //        //Get userid
        //        var userId = jsonToken?.Claims.First(claim => claim.Type == "NameIdentifier").Value;

        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            return BadRequest("Invalid token");
        //        }

        //        var therapist = _db.TherapistUsers.Where(t => t.Id == therapistId).FirstOrDefault();
        //        if (therapist == null)
        //        {
        //            return BadRequest("Therapist Not Found");
        //        }

        //        // Створення користувацького об'єкта
        //        //var newMeeting = new Meeting()
        //        //{
        //        //    Title = model.Title,   
        //        //    Url = model.Url,
        //        //    StartDate = model.StartDate,   
        //        //    IsCancelled = false,
        //        //    ClientId = userId,
        //        //    Client = _db.Users.Where(u => u.Id == userId).FirstOrDefault(),
        //        //    TherapistId = therapistId,
        //        //    Therapist = therapist
        //        //};

        //        // Збереження користувацького об'єкта
        //        //await _db.Meetings.AddAsync(newMeeting);
        //        //await _db.SaveChangesAsync();

        //        //return Ok(newMeeting);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Internal server error");
        //    }
        //}


        //GET: api/Meeting/id
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetMeeting(int id)
        {
            var meeting = await _db.Meetings.Where(x => x.Id == id).SingleOrDefaultAsync();

            return Ok(meeting);
        }

        //GET: api/Meeting
        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meeting = await _db.Meetings.ToListAsync();

            return Ok(meeting);
        }
    }
}
