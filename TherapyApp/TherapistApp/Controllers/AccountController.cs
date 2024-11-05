using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Linq;
using TherapyApp.Entities;
using TherapyApp.Helpers.Auth;
using TherapyApp.Helpers.Dto;
using TherapyApp.Services;

namespace TherapyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accService;
        private readonly IJWTService _jWTManager;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TherapyDbContext _db;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMapper mapper,
                               IAccountService accService, IJWTService jWTManager, TherapyDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _accService = accService;
            _jWTManager = jWTManager;
            _mapper = mapper;
            _db = db;
        }

        // GET api/Account/roles
        [HttpGet("roles")]
        public async Task<IActionResult> CheckRoles()
        {
            var roleList = await _accService.GetRoles();

            return Ok(roleList);
        }

        // GET: api/Account/user/id
        [HttpGet]
        [Route("user/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            AppUser user = await _accService.GetUserAsync(id);

            if (user == null)
            {
                throw new Exception("User Not Found");
            }

            return Ok(user);
        }   

        // POST api/Account/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(Register model)
        {
            await CheckRoles();

            string email = await _accService.GetUserEmail(model.Email);

            if (email == null)
            {
                if (ModelState.IsValid)
                {
                    var user = new AppUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        FatherName = model.FatherName,
                        Email = model.Email,
                        RegisteredOn = DateTime.Now,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, model.RoleName);

                        if (model.RoleName == "Therapist")
                        {
                            var newTherapist = new Therapist
                            {
                                Introduction = model.Introduction ?? string.Empty,
                                SpecialityId = model.SpecialityId,
                                UserId = user.Id
                            };
                            await _db.TherapistUsers.AddAsync(newTherapist);
                        }

                        try
                        {
                            await _accService.SaveAllAsync();
                        }
                        catch (Exception)
                        {
                            await _userManager.DeleteAsync(user);
                            throw;
                        }

                        return NoContent();
                    }

                    return BadRequest("Register failed");

                }
            }

            return BadRequest("Email is already exists");
        }

        // POST api/        /authenticate
        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public async Task<ActionResult> Authenticate([FromBody] Login model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ??
               throw new ArgumentException("User not found");

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, true);
            if (!signInResult.Succeeded)
            {
                throw new InvalidOperationException("Incorrect login data");
            }

            var jwtResult = await _jWTManager.CreateJwtTokenAsync(user) ??
                throw new InvalidOperationException("Jwt token generation failed");


            var userData = _mapper.Map<UserDataDto>(user) ??
                throw new InvalidOperationException("Mapping from User to UserDataDto failed");


            if (jwtResult.TokenString == null)
            {
                throw new InvalidOperationException("Invalid token ");
            }

            var result = new LoginResultDto
            {
                Token = jwtResult.TokenString,
                User = userData
            };

            return Ok(result);
        }

        // GET api/Account/users
        [HttpGet]
        [Route("users")]
        public async Task<List<AppUser>> GetAllUsers()
        {
            var userList = await _db.Users.ToListAsync();
            return userList;
        }

        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet]
        [Route("therapists")]
        public Task<IActionResult> GetTherapists(int page = 1, int pageSize = 10, string? specialization = null)
        {
            try
            {
                // Fetch all therapists in the role "Therapist"
                var therapistsQuery = _userManager.GetUsersInRoleAsync("Therapist").Result.AsQueryable();

                // Apply specialization filter if provided
                if (!string.IsNullOrEmpty(specialization))
                {
                    therapistsQuery = therapistsQuery.Where(t =>
                        _db.TherapistUsers.Any(tu => tu.UserId == t.Id &&
                            _db.Specialities.Any(s => s.Id == tu.SpecialityId && s.Name == specialization)));
                }

                // Get the total count of therapists matching the filter (before pagination)
                var totalTherapists = therapistsQuery.Count();

                // Apply pagination
                var paginatedTherapists = therapistsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Transform the result into a list of therapists with necessary fields
                var therapistList = paginatedTherapists
                    .Select(t => new
                    {
                        t.Id,
                        t.FirstName,
                        t.LastName,
                        Introduction = _db.TherapistUsers
                            .Where(x => x.UserId == t.Id)
                            .Select(x => x.Introduction)
                            .FirstOrDefault(),
                        SpecialityName = _db.Specialities
                            .Where(x => x.Id == _db.TherapistUsers
                                .Where(x => x.UserId == t.Id)
                                .Select(x => x.SpecialityId)
                                .FirstOrDefault())
                            .Select(x => x.Name)
                            .FirstOrDefault()
                    })
                    .ToList();

                // Return the paginated list and total count
                return Task.FromResult<IActionResult>(Ok(new
                {
                    therapists = therapistList,
                    total = totalTherapists
                }));
            }
            catch (Exception)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, "An error occurred while retrieving therapists."));
            }
        }
        [HttpGet]
        [Route("specs")]
        public async Task<IActionResult> GetSpecializations()
        {
            var specList = await _db.Specialities.
                Select(x => x.Name).
                ToListAsync();

            return Ok(specList);
        }
    }

}
