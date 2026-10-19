using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    /// <summary>
    /// Represents a race category such as age group or distance (e.g. Under 20, 10km).
    /// </summary>
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal DistanceKm { get; set; }

        // Navigation
        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }
}
