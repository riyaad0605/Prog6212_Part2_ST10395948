using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    /// <summary>
    /// Tests for ResultsController — capturing results and role enforcement.
    /// </summary>
    public class ResultsControllerTests
    {
        [Fact]
        public async Task CaptureResult_AsOrganiser_Returns201()
        {
            var context = TestHelper.GetSeededContext("results_capture_ok");
            var controller = new ResultsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(1, "Organiser")
            };

            var dto = new ResultDTO
            {
                EnrolmentId = 5,  // Sipho in EventCategory 4 (Event 2, owned by Organiser 1, no existing result)
                FinishTimeSeconds = 3200,
                Position = 5
            };

            var result = await controller.CaptureResult(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var captured = Assert.IsType<ResultResponseDTO>(created.Value);
            Assert.Equal(3200, captured.FinishTimeSeconds);
            Assert.Equal(5, captured.Position);
        }

        [Fact]
        public async Task CaptureResult_AsParticipant_Returns403()
        {
            var context = TestHelper.GetSeededContext("results_capture_part");
            var controller = new ResultsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            var dto = new ResultDTO
            {
                EnrolmentId = 1,
                FinishTimeSeconds = 3200,
                Position = 1
            };

            var result = await controller.CaptureResult(dto);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_DuplicateResult_Returns400()
        {
            var context = TestHelper.GetSeededContext("results_capture_dupe");
            var controller = new ResultsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(1, "Organiser")
            };

            // First capture
            var dto = new ResultDTO
            {
                EnrolmentId = 1,
                FinishTimeSeconds = 3200,
                Position = 1
            };
            await controller.CaptureResult(dto);

            // Second capture of same enrolment
            var result = await controller.CaptureResult(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CaptureResult_OtherOrganisersEvent_Returns403()
        {
            var context = TestHelper.GetSeededContext("results_capture_notowner");
            var controller = new ResultsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(2, "Organiser")  // Naledi can't capture results for Thabo's event
            };

            var dto = new ResultDTO
            {
                EnrolmentId = 1,  // Belongs to Event 1, owned by Organiser 1
                FinishTimeSeconds = 3000,
                Position = 1
            };

            var result = await controller.CaptureResult(dto);
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetByEvent_ReturnsResults()
        {
            var context = TestHelper.GetSeededContext("results_getbyevent");
            var controller = new ResultsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = TestHelper.CreateHttpContext(3, "Participant")
            };

            // Event 1 already has 2 seeded results (EnrolmentId 1 and 3)
            var result = await controller.GetByEvent(1);
            var ok = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<IEnumerable<ResultResponseDTO>>(ok.Value);
            Assert.Equal(2, results.Count());  // 2 seeded results for Event 1
        }
    }
}
