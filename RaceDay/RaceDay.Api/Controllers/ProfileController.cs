using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRole] 
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Reads logged-in user ID directly from the session
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                return Unauthorized(new { message = "Not authenticated." });
            }

            AppUser user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == sessionUserId.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            UserProfileDto dto = new UserProfileDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.RoleName,
                ProfilePicUrl = user.ProfilePicUrl
            };

            return Ok(dto);
        }

        // PUT: api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                return Unauthorized(new { message = "Not authenticated." });
            }

            AppUser user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == sessionUserId.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Updates allowed fields
            user.FullName = request.FullName;
            user.ProfilePicUrl = request.ProfilePicUrl;

            await _context.SaveChangesAsync();

            UserProfileDto dto = new UserProfileDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.RoleName,
                ProfilePicUrl = user.ProfilePicUrl
            };

            return Ok(dto);
        }
    }
}