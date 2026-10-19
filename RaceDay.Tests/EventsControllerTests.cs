using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    /// <summary>
    /// Tests for EventsController — CRUD operations and role enforcement.
    /// </summary>
    public class EventsControllerTests
    {
        [Fact]
        public async Task GetAll_AsOrganiser_ReturnsAllEvents()
        {
            var context = TestHelper.GetSeededContext("events_getall_org");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(1, "Organiser")
            };

            var result = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<IEnumerable<EventResponseDTO>>(ok.Value);
            Assert.Equal(3, events.Count());
        }

        [Fact]
        public async Task GetAll_AsParticipant_ReturnsOnlyPublished()
        {
            var context = TestHelper.GetSeededContext("events_getall_part");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            var result = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<IEnumerable<EventResponseDTO>>(ok.Value);
            // Only Joburg Marathon and Cape Winelands are Published
            Assert.Equal(2, events.Count());
        }

        [Fact]
        public async Task GetAll_NotLoggedIn_Returns401()
        {
            var context = TestHelper.GetSeededContext("events_getall_noauth");
            var controller = new EventsController(context);
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.GetAll();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Create_AsOrganiser_Returns201()
        {
            var context = TestHelper.GetSeededContext("events_create_ok");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(1, "Organiser")
            };

            var dto = new EventDTO
            {
                Name = "Durban Beachfront 10K",
                Description = "Scenic run along Durban's Golden Mile",
                EventDate = DateTime.UtcNow.AddMonths(4),
                Location = "Durban, KwaZulu-Natal",
                EventType = "Run",
                Status = "Draft"
            };

            var result = await controller.Create(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var ev = Assert.IsType<EventResponseDTO>(created.Value);
            Assert.Equal("Durban Beachfront 10K", ev.Name);
            Assert.Equal(1, ev.OrganiserId);
        }

        [Fact]
        public async Task Create_AsParticipant_Returns403()
        {
            var context = TestHelper.GetSeededContext("events_create_part");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            var dto = new EventDTO
            {
                Name = "Illegal Event",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Nowhere",
                EventType = "Run"
            };

            var result = await controller.Create(dto);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task Update_OwnEvent_Returns200()
        {
            var context = TestHelper.GetSeededContext("events_update_ok");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(1, "Organiser")
            };

            var dto = new EventDTO
            {
                Name = "Updated Marathon Name",
                EventDate = DateTime.UtcNow.AddMonths(3),
                Location = "Johannesburg, Gauteng",
                EventType = "Run",
                Status = "Published"
            };

            var result = await controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            var ev = Assert.IsType<EventResponseDTO>(ok.Value);
            Assert.Equal("Updated Marathon Name", ev.Name);
        }

        [Fact]
        public async Task Update_OtherOrganisersEvent_Returns403()
        {
            var context = TestHelper.GetSeededContext("events_update_notowner");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(2, "Organiser")  // Naledi trying to edit Thabo's event
            };

            var dto = new EventDTO
            {
                Name = "Hijacked Event",
                EventDate = DateTime.UtcNow,
                Location = "Nowhere",
                EventType = "Run"
            };

            var result = await controller.Update(1, dto);  // Event 1 belongs to Organiser 1
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task Delete_OwnEvent_Returns204()
        {
            var context = TestHelper.GetSeededContext("events_delete_ok");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(1, "Organiser")
            };

            var result = await controller.Delete(2);  // Soweto Cycling Classic
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_AsParticipant_Returns403()
        {
            var context = TestHelper.GetSeededContext("events_delete_part");
            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            var result = await controller.Delete(1);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }
    }
}
