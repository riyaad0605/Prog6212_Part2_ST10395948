using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    /// <summary>
    /// Stores the race result for a specific Enrolment (one result per enrolment).
    /// </summary>
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        /// <summary>
        /// Finish time stored as total seconds for easy sorting and comparison.
        /// </summary>
        [Required]
        public double FinishTimeSeconds { get; set; }

        public int? Position { get; set; }

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }
    }
}
