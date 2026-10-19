using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Capture and view race results. Organisers capture results; Participants view their own.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Capture a race result for an enrolled participant. Organisers only.
        /// </summary>
        /// <param name="dto">Result data including enrolment ID, finish time, and position.</param>
        /// <returns>The captured result.</returns>
        /// <response code="201">Result captured.</response>
        /// <response code="400">Result already exists for this enrolment.</response>
        /// <response code="403">Only Organisers can capture results.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ResultResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CaptureResult([FromBody] ResultDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can capture results." });

            // Verify enrolment exists
            var enrolment = await _context.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.EventCategory)
                    .ThenInclude(ec => ec!.Event)
                .FirstOrDefaultAsync(en => en.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });

            // Check that this organiser owns the event
            if (enrolment.EventCategory!.Event!.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only capture results for your own events." });

            // Check if a result already exists
            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentId == dto.EnrolmentId);
            if (existingResult != null)
                return BadRequest(new { message = "A result has already been captured for this enrolment." });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTimeSeconds = dto.FinishTimeSeconds,
                Position = dto.Position,
                CapturedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEvent), new { eventId = enrolment.EventCategory.EventId },
                new ResultResponseDTO
                {
                    ResultId = result.ResultId,
                    EnrolmentId = result.EnrolmentId,
                    ParticipantName = enrolment.Participant!.FirstName + " " + enrolment.Participant.LastName,
                    FinishTimeSeconds = result.FinishTimeSeconds,
                    Position = result.Position,
                    CapturedAt = result.CapturedAt
                });
        }

        /// <summary>
        /// Get all results for a specific event. Available to all authenticated users.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>A list of results for the event.</returns>
        [HttpGet("event/{eventId}")]
        [ProducesResponseType(typeof(List<ResultResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.EventCategory)
                .Where(r => r.Enrolment!.EventCategory!.EventId == eventId)
                .Select(r => new ResultResponseDTO
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName,
                    FinishTimeSeconds = r.FinishTimeSeconds,
                    Position = r.Position,
                    CapturedAt = r.CapturedAt
                }).ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Update a previously captured result. Organisers only.
        /// </summary>
        /// <param name="id">The result ID.</param>
        /// <param name="dto">Updated result data.</param>
        /// <returns>The updated result.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResultResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can update results." });

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.EventCategory)
                        .ThenInclude(ec => ec!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null) return NotFound(new { message = "Result not found." });

            if (result.Enrolment!.EventCategory!.Event!.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only update results for your own events." });

            result.FinishTimeSeconds = dto.FinishTimeSeconds;
            result.Position = dto.Position;

            await _context.SaveChangesAsync();

            return Ok(new ResultResponseDTO
            {
                ResultId = result.ResultId,
                EnrolmentId = result.EnrolmentId,
                ParticipantName = result.Enrolment.Participant!.FirstName + " " + result.Enrolment.Participant.LastName,
                FinishTimeSeconds = result.FinishTimeSeconds,
                Position = result.Position,
                CapturedAt = result.CapturedAt
            });
        }
    }
}
