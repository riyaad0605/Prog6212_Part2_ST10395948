using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Manage participant enrolments into event categories.
    /// Participants can enrol and withdraw; Organisers can view enrolments for their events.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Enrol the current Participant into an event category. Checks capacity limits.
        /// </summary>
        /// <param name="dto">The event category to enrol in.</param>
        /// <returns>The enrolment confirmation.</returns>
        /// <response code="201">Enrolled successfully.</response>
        /// <response code="400">Already enrolled or category is full.</response>
        /// <response code="403">Only Participants can enrol.</response>
        [HttpPost]
        [ProducesResponseType(typeof(EnrolmentResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Enrol([FromBody] EnrolmentDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Participant")
                return StatusCode(403, new { message = "Only Participants can enrol in events." });

            // Verify event category exists
            var ec = await _context.EventCategories
                .Include(x => x.Event)
                .Include(x => x.Category)
                .Include(x => x.Enrolments)
                .FirstOrDefaultAsync(x => x.EventCategoryId == dto.EventCategoryId);

            if (ec == null) return NotFound(new { message = "Event category not found." });

            // Check if event is published
            if (ec.Event!.Status != "Published")
                return BadRequest(new { message = "This event is not open for enrolment." });

            // Check if already enrolled
            var alreadyEnrolled = ec.Enrolments
                .Any(en => en.ParticipantId == userId.Value && en.Status == "Confirmed");
            if (alreadyEnrolled)
                return BadRequest(new { message = "You are already enrolled in this category." });

            // Check capacity
            var confirmedCount = ec.Enrolments.Count(en => en.Status == "Confirmed");
            if (confirmedCount >= ec.MaxParticipants)
                return BadRequest(new { message = "This category has reached its maximum capacity." });

            var enrolment = new Enrolment
            {
                ParticipantId = userId.Value,
                EventCategoryId = dto.EventCategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId.Value);

            return CreatedAtAction(nameof(GetMyEnrolments), null, new EnrolmentResponseDTO
            {
                EnrolmentId = enrolment.EnrolmentId,
                ParticipantId = enrolment.ParticipantId,
                ParticipantName = user!.FirstName + " " + user.LastName,
                EventCategoryId = enrolment.EventCategoryId,
                EventName = ec.Event.Name,
                CategoryName = ec.Category!.Name,
                EnrolmentDate = enrolment.EnrolmentDate,
                Status = enrolment.Status
            });
        }

        /// <summary>
        /// Get all enrolments for the currently logged-in Participant.
        /// </summary>
        /// <returns>A list of the Participant's enrolments.</returns>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<EnrolmentResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var enrolments = await _context.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.EventCategory)
                    .ThenInclude(ec => ec!.Event)
                .Include(en => en.EventCategory)
                    .ThenInclude(ec => ec!.Category)
                .Where(en => en.ParticipantId == userId.Value)
                .Select(en => new EnrolmentResponseDTO
                {
                    EnrolmentId = en.EnrolmentId,
                    ParticipantId = en.ParticipantId,
                    ParticipantName = en.Participant!.FirstName + " " + en.Participant.LastName,
                    EventCategoryId = en.EventCategoryId,
                    EventName = en.EventCategory!.Event!.Name,
                    CategoryName = en.EventCategory.Category!.Name,
                    EnrolmentDate = en.EnrolmentDate,
                    Status = en.Status
                }).ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Get all enrolments for a specific event. Organisers see all; Participants see their own.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>A list of enrolments for the event.</returns>
        [HttpGet("event/{eventId}")]
        [ProducesResponseType(typeof(List<EnrolmentResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");

            var query = _context.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.EventCategory)
                    .ThenInclude(ec => ec!.Event)
                .Include(en => en.EventCategory)
                    .ThenInclude(ec => ec!.Category)
                .Where(en => en.EventCategory!.EventId == eventId);

            // Participants can only see their own enrolments
            if (role == "Participant")
            {
                query = query.Where(en => en.ParticipantId == userId.Value);
            }

            var enrolments = await query.Select(en => new EnrolmentResponseDTO
            {
                EnrolmentId = en.EnrolmentId,
                ParticipantId = en.ParticipantId,
                ParticipantName = en.Participant!.FirstName + " " + en.Participant.LastName,
                EventCategoryId = en.EventCategoryId,
                EventName = en.EventCategory!.Event!.Name,
                CategoryName = en.EventCategory.Category!.Name,
                EnrolmentDate = en.EnrolmentDate,
                Status = en.Status
            }).ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Withdraw from an event category. Only the enrolled Participant can withdraw.
        /// </summary>
        /// <param name="id">The enrolment ID to withdraw.</param>
        /// <response code="204">Withdrawn successfully.</response>
        /// <response code="403">Not your enrolment.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });

            if (enrolment.ParticipantId != userId.Value)
                return StatusCode(403, new { message = "You can only withdraw your own enrolments." });

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
