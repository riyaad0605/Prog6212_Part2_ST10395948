using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manage user profile information. Requires an active session.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the currently logged-in user's profile.
        /// </summary>
        /// <returns>The user profile.</returns>
        /// <response code="200">Profile returned.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            return Ok(new UserResponseDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            });
        }

        /// <summary>
        /// Update the currently logged-in user's first and last name.
        /// </summary>
        /// <param name="dto">Updated profile data.</param>
        /// <returns>The updated profile.</returns>
        /// <response code="200">Profile updated.</response>
        /// <response code="401">Not logged in.</response>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            });
        }
    }
}
