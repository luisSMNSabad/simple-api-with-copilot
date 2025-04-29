// Tests/AuthenticationTests.cs
using NUnit.Framework;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using SecureApp.Constants;
using SecureApp.Controllers;
using SecureApp.Models;
using SecureApp.Services;
using SecureApp.Models.Auth;

namespace SecureApp.Tests
{
    [TestFixture]
    public class AuthenticationTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<IConfiguration> _mockConfiguration;
        private AuthService _authService;
        private RoleService _roleService;
        private AuthController _authController;
        private AdminController _adminController;

        [SetUp]
        public void Setup()
        {
            // Setup UserManager mock
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object,
                null, null, null, null, null, null, null, null);

            // Setup RoleManager mock
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            // Setup Configuration mock
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-with-at-least-32-characters");
            _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("test");
            _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("test");

            // Initialize services
            _authService = new AuthService(_mockUserManager.Object, _mockConfiguration.Object);
            _roleService = new RoleService(_mockUserManager.Object, _mockRoleManager.Object);

            // Initialize controllers
            var validator = new InputValidator();
            _authController = new AuthController(_authService, validator);
            _adminController = new AdminController(_roleService, _authService);
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@test.com" };
            var loginRequest = new LoginRequest { Username = "testuser", Password = "Password123!" };

            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { Roles.User });

            // Act
            var result = await _authController.Login(loginRequest) as ObjectResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.Not.Null);
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "testuser", Password = "wrongpassword" };

            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authController.Login(loginRequest) as BadRequestObjectResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task AssignRole_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var userId = "testUserId";
            var role = Roles.Admin;

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());
            _mockRoleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _adminController.AssignRole(userId, role) as ObjectResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task RemoveRole_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var userId = "testUserId";
            var role = Roles.Admin;

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());
            _mockUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _adminController.RemoveRole(userId, role) as ObjectResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Login_WithSQLInjectionAttempt_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "' OR '1'='1",
                Password = "' OR '1'='1"
            };

            // Act
            var result = await _authController.Login(loginRequest) as BadRequestObjectResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task GetUserRoles_WithValidUser_ReturnsRoles()
        {
            // Arrange
            var userId = "testUserId";
            var expectedRoles = new List<string> { Roles.User };

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());
            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(expectedRoles);

            // Act
            var result = await _adminController.GetUserRoles(userId) as ObjectResult;
            var roles = result.Value as IList<string>;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(roles, Is.EqualTo(expectedRoles));
        }
    }
}