using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    /// <summary>
    /// Represents a race day event created by an Organiser.
    /// </summary>
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Type of event: Run, Walk, or Cycle.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Event status: Draft, Published, Completed, or Cancelled.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrganiserId")]
        public User? Organiser { get; set; }

        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
