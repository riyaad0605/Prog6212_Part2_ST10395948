using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.Tests.Helpers
{
    /// <summary>
    /// Provides shared setup methods for unit tests — in-memory database
    /// and a fake HttpContext with session support.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Creates a fresh in-memory RaceDayDbContext and seeds it with test data.
        /// </summary>
        public static RaceDayDbContext GetSeededContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // In-memory provider runs OnModelCreating which seeds data
            // If seed data didn't get applied, add it manually
            if (!context.Users.Any())
            {
                var hash = BCrypt.Net.BCrypt.HashPassword("Pass123!");
                context.Users.AddRange(
                    new User { UserId = 1, Email = "organiser1@raceday.co.za", PasswordHash = hash, FirstName = "Thabo", LastName = "Mokoena", Role = "Organiser", CreatedAt = DateTime.UtcNow },
                    new User { UserId = 2, Email = "organiser2@raceday.co.za", PasswordHash = hash, FirstName = "Naledi", LastName = "Dlamini", Role = "Organiser", CreatedAt = DateTime.UtcNow },
                    new User { UserId = 3, Email = "runner1@raceday.co.za", PasswordHash = hash, FirstName = "Sipho", LastName = "Nkosi", Role = "Participant", CreatedAt = DateTime.UtcNow },
                    new User { UserId = 4, Email = "runner2@raceday.co.za", PasswordHash = hash, FirstName = "Lerato", LastName = "Molefe", Role = "Participant", CreatedAt = DateTime.UtcNow }
                );

                context.Events.AddRange(
                    new Event { EventId = 1, OrganiserId = 1, Name = "Joburg City Marathon", Description = "Annual road race", EventDate = DateTime.UtcNow.AddMonths(3), Location = "Johannesburg, Gauteng", EventType = "Run", Status = "Published", CreatedAt = DateTime.UtcNow },
                    new Event { EventId = 2, OrganiserId = 1, Name = "Soweto Cycling Classic", Description = "Scenic cycle", EventDate = DateTime.UtcNow.AddMonths(5), Location = "Soweto, Gauteng", EventType = "Cycle", Status = "Draft", CreatedAt = DateTime.UtcNow },
                    new Event { EventId = 3, OrganiserId = 2, Name = "Cape Winelands Fun Walk", Description = "Vineyard walk", EventDate = DateTime.UtcNow.AddMonths(6), Location = "Stellenbosch, Western Cape", EventType = "Walk", Status = "Published", CreatedAt = DateTime.UtcNow }
                );

                context.Categories.AddRange(
                    new Category { CategoryId = 1, Name = "10km", Description = "Standard 10km", DistanceKm = 10 },
                    new Category { CategoryId = 2, Name = "21km Half Marathon", Description = "Half marathon", DistanceKm = 21.1m },
                    new Category { CategoryId = 3, Name = "42km Full Marathon", Description = "Full marathon", DistanceKm = 42.2m },
                    new Category { CategoryId = 4, Name = "5km Fun Run", Description = "Short distance", DistanceKm = 5 }
                );

                context.EventCategories.AddRange(
                    new EventCategory { EventCategoryId = 1, EventId = 1, CategoryId = 1, MaxParticipants = 500 },
                    new EventCategory { EventCategoryId = 2, EventId = 1, CategoryId = 2, MaxParticipants = 300 },
                    new EventCategory { EventCategoryId = 3, EventId = 3, CategoryId = 4, MaxParticipants = 600 }
                );

                context.Enrolments.Add(
                    new Enrolment { EnrolmentId = 1, ParticipantId = 3, EventCategoryId = 1, EnrolmentDate = DateTime.UtcNow, Status = "Confirmed" }
                );

                context.SaveChanges();
            }

            return context;
        }

        /// <summary>
        /// Creates a mock HttpContext with session values set for a given user.
        /// </summary>
        public static DefaultHttpContext CreateHttpContext(int userId, string role, string email = "test@test.com")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", userId);
            httpContext.Session.SetString("Role", role);
            httpContext.Session.SetString("Email", email);
            return httpContext;
        }
    }

    /// <summary>
    /// A simple in-memory session implementation for testing purposes.
    /// </summary>
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[]? value)
            => _store.TryGetValue(key, out value);
    }
}
