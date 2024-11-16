using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TherapyApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Security.Claims;

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
        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateMeeting([FromBody] Meeting model, int therapistId)
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

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid token");
                }

                var newMeeting = new Meeting()
                {
                    Title = model.Title,
                    Url = model.Url,
                    StartDate = model.StartDate,
                    IsCancelled = false,
                    ClientId = userId,
                    Client = _db.Users.Where(u => u.Id == userId).FirstOrDefault(),
                    TherapistId = model.TherapistId,
                    Therapist = _db.Users.Where(u => u.Id == model.TherapistId).FirstOrDefault()
                };

                await _db.Meetings.AddAsync(newMeeting);
                await _db.SaveChangesAsync();

                return Ok(newMeeting);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


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

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetUserMeetings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var meetings = await _db.Meetings
                .Where(m => m.ClientId.Equals(userId) || m.TherapistId.Equals(userId))
                .ToListAsync();
            return Ok(meetings);
        }

        [Authorize]
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteMeetingById(int id)
        {

            var meetingToDelete = await _db.Meetings.Where(d => d.Id == id)
                                                     .FirstOrDefaultAsync();

            if(meetingToDelete == null)
            {
                return NotFound();
            }

            _db.Meetings.Remove(meetingToDelete);
            _db.SaveChanges();

            return Ok();
        }

        [Authorize]
        [HttpGet("bookedDates/{id}")]
        public async Task<IActionResult> GetBookedDates(string therapistId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookedDates = await _db.Meetings
                .Where(m => m.ClientId == userId 
                         || m.TherapistId == therapistId 
                         || m.TherapistId == userId
                )
                .Select(m => m.StartDate)
                .Distinct()
                .ToListAsync();

            return Ok(bookedDates);
        }
    }
}
