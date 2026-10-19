using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Full CRUD for race day events. Organisers can create, update, and delete
    /// their own events. All authenticated users can browse events.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// List all events. Participants see only Published events; Organisers see all.
        /// </summary>
        /// <returns>A list of events.</returns>
        /// <response code="200">Events returned.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<EventResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");

            IQueryable<Event> query = _context.Events.Include(e => e.Organiser);

            // Participants only see published events
            if (role == "Participant")
            {
                query = query.Where(e => e.Status == "Published");
            }

            var events = await query.Select(e => new EventResponseDTO
            {
                EventId = e.EventId,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser!.FirstName + " " + e.Organiser.LastName,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                EventType = e.EventType,
                Status = e.Status,
                CreatedAt = e.CreatedAt
            }).ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Get a single event by its ID.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>The event details.</returns>
        /// <response code="200">Event found.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var ev = await _context.Events.Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });

            return Ok(new EventResponseDTO
            {
                EventId = ev.EventId,
                OrganiserId = ev.OrganiserId,
                OrganiserName = ev.Organiser!.FirstName + " " + ev.Organiser.LastName,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                EventType = ev.EventType,
                Status = ev.Status,
                CreatedAt = ev.CreatedAt
            });
        }

        /// <summary>
        /// Create a new event. Only Organisers can do this.
        /// </summary>
        /// <param name="dto">Event details.</param>
        /// <returns>The newly created event.</returns>
        /// <response code="201">Event created.</response>
        /// <response code="403">Only Organisers can create events.</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] EventDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can create events." });

            // Validate event type
            var validTypes = new[] { "Run", "Walk", "Cycle" };
            if (!validTypes.Contains(dto.EventType))
                return BadRequest(new { message = "EventType must be Run, Walk, or Cycle." });

            var ev = new Event
            {
                OrganiserId = userId.Value,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                EventType = dto.EventType,
                Status = dto.Status ?? "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            // Reload with organiser for the response
            await _context.Entry(ev).Reference(e => e.Organiser).LoadAsync();

            var response = new EventResponseDTO
            {
                EventId = ev.EventId,
                OrganiserId = ev.OrganiserId,
                OrganiserName = ev.Organiser!.FirstName + " " + ev.Organiser.LastName,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                EventType = ev.EventType,
                Status = ev.Status,
                CreatedAt = ev.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, response);
        }

        /// <summary>
        /// Update an existing event. Only the Organiser who created it can update.
        /// </summary>
        /// <param name="id">The event ID to update.</param>
        /// <param name="dto">Updated event details.</param>
        /// <returns>The updated event.</returns>
        /// <response code="200">Event updated.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EventResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] EventDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can update events." });

            var ev = await _context.Events.Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only update your own events." });

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.EventType = dto.EventType;
            ev.Status = dto.Status ?? ev.Status;

            await _context.SaveChangesAsync();

            return Ok(new EventResponseDTO
            {
                EventId = ev.EventId,
                OrganiserId = ev.OrganiserId,
                OrganiserName = ev.Organiser!.FirstName + " " + ev.Organiser.LastName,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                EventType = ev.EventType,
                Status = ev.Status,
                CreatedAt = ev.CreatedAt
            });
        }

        /// <summary>
        /// Delete an event. Only the Organiser who created it can delete.
        /// </summary>
        /// <param name="id">The event ID to delete.</param>
        /// <response code="204">Event deleted.</response>
        /// <response code="403">Not the event owner.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can delete events." });

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only delete your own events." });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
