using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    /// <summary>
    /// Link categories to events and manage participant caps. Organisers only.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventCategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventCategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all categories linked to a specific event, with current enrolment counts.
        /// </summary>
        /// <param name="eventId">The event ID to get categories for.</param>
        /// <returns>A list of event-category links with enrolment counts.</returns>
        [HttpGet("event/{eventId}")]
        [ProducesResponseType(typeof(List<EventCategoryResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var items = await _context.EventCategories
                .Include(ec => ec.Event)
                .Include(ec => ec.Category)
                .Include(ec => ec.Enrolments)
                .Where(ec => ec.EventId == eventId)
                .Select(ec => new EventCategoryResponseDTO
                {
                    EventCategoryId = ec.EventCategoryId,
                    EventId = ec.EventId,
                    EventName = ec.Event!.Name,
                    CategoryId = ec.CategoryId,
                    CategoryName = ec.Category!.Name,
                    MaxParticipants = ec.MaxParticipants,
                    CurrentEnrolments = ec.Enrolments.Count(en => en.Status == "Confirmed")
                }).ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Link a category to an event. Only the event's Organiser can do this.
        /// </summary>
        /// <param name="dto">The event ID, category ID, and max participants.</param>
        /// <returns>The newly created link.</returns>
        /// <response code="201">Category linked to event.</response>
        /// <response code="403">Only the event Organiser can manage event categories.</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventCategoryResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] EventCategoryDTO dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can manage event categories." });

            // Verify the event belongs to this organiser
            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only manage categories for your own events." });

            // Verify category exists
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null) return NotFound(new { message = "Category not found." });

            var ec = new EventCategory
            {
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                MaxParticipants = dto.MaxParticipants
            };

            _context.EventCategories.Add(ec);
            await _context.SaveChangesAsync();

            var response = new EventCategoryResponseDTO
            {
                EventCategoryId = ec.EventCategoryId,
                EventId = ec.EventId,
                EventName = ev.Name,
                CategoryId = ec.CategoryId,
                CategoryName = category.Name,
                MaxParticipants = ec.MaxParticipants,
                CurrentEnrolments = 0
            };

            return CreatedAtAction(nameof(GetByEvent), new { eventId = ec.EventId }, response);
        }

        /// <summary>
        /// Remove a category from an event. Only the event's Organiser can do this.
        /// </summary>
        /// <param name="id">The EventCategory ID to remove.</param>
        /// <response code="204">Link removed.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized(new { message = "Please log in first." });

            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { message = "Only Organisers can manage event categories." });

            var ec = await _context.EventCategories
                .Include(x => x.Event)
                .FirstOrDefaultAsync(x => x.EventCategoryId == id);

            if (ec == null) return NotFound(new { message = "Event category not found." });
            if (ec.Event!.OrganiserId != userId.Value)
                return StatusCode(403, new { message = "You can only manage categories for your own events." });

            _context.EventCategories.Remove(ec);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
