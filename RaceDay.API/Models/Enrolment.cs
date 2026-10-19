using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    /// <summary>
    /// Represents a Participant's enrolment into a specific EventCategory.
    /// </summary>
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int EventCategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Enrolment status: Confirmed or Withdrawn.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Confirmed";

        // Navigation
        [ForeignKey("ParticipantId")]
        public User? Participant { get; set; }

        [ForeignKey("EventCategoryId")]
        public EventCategory? EventCategory { get; set; }

        public Result? Result { get; set; }
    }
}
