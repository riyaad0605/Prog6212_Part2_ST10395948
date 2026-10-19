using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    /// <summary>
    /// Entity Framework database context for the RaceDay application.
    /// Uses Code-First approach with SQL Server.
    /// </summary>
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role)
                    .HasMaxLength(20);
            });

            // Event configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasOne(e => e.Organiser)
                    .WithMany(u => u.OrganisedEvents)
                    .HasForeignKey(e => e.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // EventCategory configuration
            modelBuilder.Entity<EventCategory>(entity =>
            {
                entity.HasOne(ec => ec.Event)
                    .WithMany(e => e.EventCategories)
                    .HasForeignKey(ec => ec.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ec => ec.Category)
                    .WithMany(c => c.EventCategories)
                    .HasForeignKey(ec => ec.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Enrolment configuration
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.HasOne(en => en.Participant)
                    .WithMany(u => u.Enrolments)
                    .HasForeignKey(en => en.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(en => en.EventCategory)
                    .WithMany(ec => ec.Enrolments)
                    .HasForeignKey(en => en.EventCategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Result configuration - one result per enrolment
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasIndex(r => r.EnrolmentId).IsUnique();

                entity.HasOne(r => r.Enrolment)
                    .WithOne(en => en.Result)
                    .HasForeignKey<Result>(r => r.EnrolmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users (passwords hashed with BCrypt)
            // Password for all seed users: "Pass123!"
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Pass123!");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "organiser1@raceday.co.za",
                    PasswordHash = hashedPassword,
                    FirstName = "Thabo",
                    LastName = "Mokoena",
                    Role = "Organiser",
                    CreatedAt = new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 2,
                    Email = "organiser2@raceday.co.za",
                    PasswordHash = hashedPassword,
                    FirstName = "Naledi",
                    LastName = "Dlamini",
                    Role = "Organiser",
                    CreatedAt = new DateTime(2024, 2, 10, 9, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 3,
                    Email = "runner1@raceday.co.za",
                    PasswordHash = hashedPassword,
                    FirstName = "Sipho",
                    LastName = "Nkosi",
                    Role = "Participant",
                    CreatedAt = new DateTime(2024, 3, 5, 10, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 4,
                    Email = "runner2@raceday.co.za",
                    PasswordHash = hashedPassword,
                    FirstName = "Lerato",
                    LastName = "Molefe",
                    Role = "Participant",
                    CreatedAt = new DateTime(2024, 3, 20, 11, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Events
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventId = 1,
                    OrganiserId = 1,
                    Name = "Joburg City Marathon",
                    Description = "Annual road race through the streets of Johannesburg CBD, passing landmarks like the Carlton Centre and Constitution Hill.",
                    EventDate = new DateTime(2025, 6, 15, 7, 0, 0, DateTimeKind.Utc),
                    Location = "Johannesburg, Gauteng",
                    EventType = "Run",
                    Status = "Published",
                    CreatedAt = new DateTime(2024, 4, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Event
                {
                    EventId = 2,
                    OrganiserId = 1,
                    Name = "Soweto Cycling Classic",
                    Description = "Scenic cycle through Soweto, passing by the Hector Pieterson Memorial and Orlando Towers.",
                    EventDate = new DateTime(2025, 8, 20, 6, 30, 0, DateTimeKind.Utc),
                    Location = "Soweto, Gauteng",
                    EventType = "Cycle",
                    Status = "Draft",
                    CreatedAt = new DateTime(2024, 5, 12, 9, 0, 0, DateTimeKind.Utc)
                },
                new Event
                {
                    EventId = 3,
                    OrganiserId = 2,
                    Name = "Cape Winelands Fun Walk",
                    Description = "A scenic walk through the vineyards of Stellenbosch and Franschhoek.",
                    EventDate = new DateTime(2025, 9, 10, 8, 0, 0, DateTimeKind.Utc),
                    Location = "Stellenbosch, Western Cape",
                    EventType = "Walk",
                    Status = "Published",
                    CreatedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "10km", Description = "Standard 10 kilometre distance", DistanceKm = 10 },
                new Category { CategoryId = 2, Name = "21km Half Marathon", Description = "Half marathon distance", DistanceKm = 21.1m },
                new Category { CategoryId = 3, Name = "42km Full Marathon", Description = "Full marathon distance", DistanceKm = 42.2m },
                new Category { CategoryId = 4, Name = "5km Fun Run", Description = "Short distance for beginners and families", DistanceKm = 5 }
            );

            // Seed EventCategories
            modelBuilder.Entity<EventCategory>().HasData(
                new EventCategory { EventCategoryId = 1, EventId = 1, CategoryId = 1, MaxParticipants = 500 },
                new EventCategory { EventCategoryId = 2, EventId = 1, CategoryId = 2, MaxParticipants = 300 },
                new EventCategory { EventCategoryId = 3, EventId = 1, CategoryId = 3, MaxParticipants = 200 },
                new EventCategory { EventCategoryId = 4, EventId = 2, CategoryId = 1, MaxParticipants = 150 },
                new EventCategory { EventCategoryId = 5, EventId = 2, CategoryId = 4, MaxParticipants = 400 },
                new EventCategory { EventCategoryId = 6, EventId = 3, CategoryId = 4, MaxParticipants = 600 },
                new EventCategory { EventCategoryId = 7, EventId = 3, CategoryId = 1, MaxParticipants = 250 }
            );

            // Seed Enrolments
            modelBuilder.Entity<Enrolment>().HasData(
                new Enrolment
                {
                    EnrolmentId = 1,
                    ParticipantId = 3,
                    EventCategoryId = 1,
                    EnrolmentDate = new DateTime(2024, 7, 1, 12, 0, 0, DateTimeKind.Utc),
                    Status = "Confirmed"
                },
                new Enrolment
                {
                    EnrolmentId = 2,
                    ParticipantId = 3,
                    EventCategoryId = 6,
                    EnrolmentDate = new DateTime(2024, 7, 5, 14, 0, 0, DateTimeKind.Utc),
                    Status = "Confirmed"
                },
                new Enrolment
                {
                    EnrolmentId = 3,
                    ParticipantId = 4,
                    EventCategoryId = 2,
                    EnrolmentDate = new DateTime(2024, 7, 10, 9, 0, 0, DateTimeKind.Utc),
                    Status = "Confirmed"
                },
                new Enrolment
                {
                    EnrolmentId = 4,
                    ParticipantId = 4,
                    EventCategoryId = 7,
                    EnrolmentDate = new DateTime(2024, 7, 15, 10, 0, 0, DateTimeKind.Utc),
                    Status = "Confirmed"
                },
                new Enrolment
                {
                    EnrolmentId = 5,
                    ParticipantId = 3,
                    EventCategoryId = 4,
                    EnrolmentDate = new DateTime(2024, 8, 1, 8, 0, 0, DateTimeKind.Utc),
                    Status = "Confirmed"
                }
            );

            // Seed Results
            modelBuilder.Entity<Result>().HasData(
                new Result
                {
                    ResultId = 1,
                    EnrolmentId = 1,
                    FinishTimeSeconds = 3120,  // 52 minutes
                    Position = 1,
                    CapturedAt = new DateTime(2025, 6, 15, 8, 52, 0, DateTimeKind.Utc)
                },
                new Result
                {
                    ResultId = 2,
                    EnrolmentId = 3,
                    FinishTimeSeconds = 6840,  // 1h54m
                    Position = 1,
                    CapturedAt = new DateTime(2025, 6, 15, 8, 54, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
