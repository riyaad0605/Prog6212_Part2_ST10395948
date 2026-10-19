using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    /// <summary>
    /// Data for creating or updating an event.
    /// </summary>
    public class EventDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Type of event: Run, Walk, or Cycle.
        /// </summary>
        [Required]
        public string EventType { get; set; } = string.Empty;

        public string Status { get; set; } = "Draft";
    }

    /// <summary>
    /// Event data sent back to the client.
    /// </summary>
    public class EventResponseDTO
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Data for creating or updating a category.
    /// </summary>
    public class CategoryDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal DistanceKm { get; set; }
    }

    /// <summary>
    /// Data for linking a category to an event.
    /// </summary>
    public class EventCategoryDTO
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int MaxParticipants { get; set; }
    }

    /// <summary>
    /// EventCategory data returned from the API.
    /// </summary>
    public class EventCategoryResponseDTO
    {
        public int EventCategoryId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public int CurrentEnrolments { get; set; }
    }

    /// <summary>
    /// Data for enrolling in an event category.
    /// </summary>
    public class EnrolmentDTO
    {
        [Required]
        public int EventCategoryId { get; set; }
    }

    /// <summary>
    /// Enrolment data returned from the API.
    /// </summary>
    public class EnrolmentResponseDTO
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventCategoryId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data for capturing a race result.
    /// </summary>
    public class ResultDTO
    {
        [Required]
        public int EnrolmentId { get; set; }

        /// <summary>
        /// Finish time in total seconds.
        /// </summary>
        [Required]
        public double FinishTimeSeconds { get; set; }

        public int? Position { get; set; }
    }

    /// <summary>
    /// Result data returned from the API.
    /// </summary>
    public class ResultResponseDTO
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public double FinishTimeSeconds { get; set; }
        public int? Position { get; set; }
        public DateTime CapturedAt { get; set; }
    }
}
