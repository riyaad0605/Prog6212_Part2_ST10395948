using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    /// <summary>
    /// Tests for the AuthController — registration and login flows.
    /// </summary>
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_ValidOrganiser_Returns201()
        {
            // Arrange
            var context = TestHelper.GetSeededContext("auth_register_ok");
            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            };

            var dto = new RegisterDTO
            {
                Email = "new.organiser@test.co.za",
                Password = "Secure123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Organiser"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var user = Assert.IsType<UserResponseDTO>(created.Value);
            Assert.Equal("new.organiser@test.co.za", user.Email);
            Assert.Equal("Organiser", user.Role);
        }

        [Fact]
        public async Task Register_ValidParticipant_Returns201()
        {
            var context = TestHelper.GetSeededContext("auth_register_participant");
            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            };

            var dto = new RegisterDTO
            {
                Email = "new.runner@test.co.za",
                Password = "Run456!",
                FirstName = "New",
                LastName = "Runner",
                Role = "Participant"
            };

            var result = await controller.Register(dto);
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            var context = TestHelper.GetSeededContext("auth_register_dupe");
            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            };

            var dto = new RegisterDTO
            {
                Email = "organiser1@raceday.co.za",  // already exists in seed
                Password = "Test123!",
                FirstName = "Dupe",
                LastName = "User",
                Role = "Organiser"
            };

            var result = await controller.Register(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            var context = TestHelper.GetSeededContext("auth_register_badrole");
            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            };

            var dto = new RegisterDTO
            {
                Email = "bad@test.co.za",
                Password = "Test123!",
                FirstName = "Bad",
                LastName = "Role",
                Role = "Admin"  // invalid
            };

            var result = await controller.Register(dto);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200()
        {
            var context = TestHelper.GetSeededContext("auth_login_ok");
            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            };

            var dto = new LoginDTO
            {
                Email = "organiser1@raceday.co.za",
                Password = "Pass123!"
            };

            var result = await controller.Login(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<UserResponseDTO>(ok.Value);
            Assert.Equal("Thabo", user.FirstName);
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            var context = TestHelper.GetSeededContext("auth_login_fail");
            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            };

            var dto = new LoginDTO
            {
                Email = "organiser1@raceday.co.za",
                Password = "WrongPass!"
            };

            var result = await controller.Login(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_NonExistentEmail_Returns401()
        {
            var context = TestHelper.GetSeededContext("auth_login_nouser");
            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            };

            var dto = new LoginDTO
            {
                Email = "nobody@raceday.co.za",
                Password = "Pass123!"
            };

            var result = await controller.Login(dto);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void Logout_ClearsSession_Returns200()
        {
            var context = TestHelper.GetSeededContext("auth_logout");
            var controller = new AuthController(context);
            var httpContext = TestHelper.CreateHttpContext(1, "Organiser");
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Logout();
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
