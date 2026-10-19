using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    /// <summary>
    /// Tests for EnrolmentsController — enrolment, withdrawal, and role enforcement.
    /// </summary>
    public class EnrolmentsControllerTests
    {
        [Fact]
        public async Task Enrol_AsParticipant_Returns201()
        {
            var context = TestHelper.GetSeededContext("enrol_ok");
            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(4, "Participant", "runner2@raceday.co.za")
            };

            var dto = new EnrolmentDTO { EventCategoryId = 1 };  // 10km at Joburg Marathon

            var result = await controller.Enrol(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var enrolment = Assert.IsType<EnrolmentResponseDTO>(created.Value);
            Assert.Equal("Confirmed", enrolment.Status);
            Assert.Equal("Lerato Molefe", enrolment.ParticipantName);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_Returns403()
        {
            var context = TestHelper.GetSeededContext("enrol_organiser");
            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(1, "Organiser")
            };

            var dto = new EnrolmentDTO { EventCategoryId = 1 };

            var result = await controller.Enrol(dto);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task Enrol_AlreadyEnrolled_Returns400()
        {
            var context = TestHelper.GetSeededContext("enrol_duplicate");
            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant", "runner1@raceday.co.za")
            };

            // Sipho is already enrolled in EventCategory 1 (seeded)
            var dto = new EnrolmentDTO { EventCategoryId = 1 };

            var result = await controller.Enrol(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Enrol_DraftEvent_Returns400()
        {
            var context = TestHelper.GetSeededContext("enrol_draft");
            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            // Need an EventCategory for Event 2 (Draft status)
            var ec = new RaceDay.API.Models.EventCategory
            {
                EventId = 2,
                CategoryId = 1,
                MaxParticipants = 100
            };
            context.EventCategories.Add(ec);
            context.SaveChanges();

            var dto = new EnrolmentDTO { EventCategoryId = ec.EventCategoryId };

            var result = await controller.Enrol(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetMyEnrolments_ReturnsOnlyOwnEnrolments()
        {
            var context = TestHelper.GetSeededContext("enrol_my");
            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            var result = await controller.GetMyEnrolments();
            var ok = Assert.IsType<OkObjectResult>(result);
            var enrolments = Assert.IsAssignableFrom<IEnumerable<EnrolmentResponseDTO>>(ok.Value);
            Assert.Equal(3, enrolments.Count());  // Sipho has 3 enrolments in seed
        }

        [Fact]
        public async Task Withdraw_OwnEnrolment_Returns204()
        {
            var context = TestHelper.GetSeededContext("enrol_withdraw");
            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            var result = await controller.Withdraw(1);  // Enrolment 1 belongs to Participant 3
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Withdraw_OtherPersonsEnrolment_Returns403()
        {
            var context = TestHelper.GetSeededContext("enrol_withdraw_other");
            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(4, "Participant")  // Lerato trying to withdraw Sipho's
            };

            var result = await controller.Withdraw(1);  // Enrolment 1 belongs to Participant 3
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }
    }
}
