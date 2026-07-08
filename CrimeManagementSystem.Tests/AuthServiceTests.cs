using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Exceptions;
using CrimeManagementSystem.Interfaces;
using CrimeManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace CrimeManagementSystem.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private AppDbContext _context = null!;
        private AuthService _authService = null!;
        private Mock<ILogger<AuthService>> _loggerMock = null!;
        private IConfiguration _config = null!;
        private Mock<IEmailService> _emailServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "TestSecretKey12345678901234567890" },
                { "JwtSettings:Issuer", "TestIssuer" },
                { "JwtSettings:Audience", "TestAudience" },
                { "JwtSettings:ExpiryMinutes", "60" }
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _loggerMock = new Mock<ILogger<AuthService>>();
            _emailServiceMock = new Mock<IEmailService>();

            _authService = new AuthService(
                _context, _config, _loggerMock.Object, _emailServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ── TEST 1: Successful Registration ───────────────────
        [Test]
        public async Task RegisterAsync_ValidUser_ReturnsAuthResponseWithToken()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Password = "Test@123",
                AadhaarNumber = "123456789012",
                PANNumber = "ABCDE1234F",
                DateOfBirth = new DateTime(1995, 6, 15),
                Address = "Chennai",
                Phone = "9876543210"
            };

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Email, Is.EqualTo("test@gmail.com"));
            Assert.That(result.Role, Is.EqualTo("User"));
        }

        // ── TEST 2: Duplicate Email Registration Fails ────────
        [Test]
        public async Task RegisterAsync_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Name = "Test User",
                Email = "duplicate@gmail.com",
                Password = "Test@123",
                AadhaarNumber = "123456789012",
                PANNumber = "ABCDE1234F",
                DateOfBirth = new DateTime(1995, 6, 15),
                Address = "Chennai",
                Phone = "9876543210"
            };
            await _authService.RegisterAsync(dto);

            // Act & Assert
            var ex = Assert.ThrowsAsync<DuplicateResourceException>(async () =>
                await _authService.RegisterAsync(dto));

            Assert.That(ex!.Message, Does.Contain("already exists"));
        }

        // ── TEST 3: Successful Login ──────────────────────────
        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Name = "Login Test",
                Email = "login@gmail.com",
                Password = "Login@123",
                AadhaarNumber = "111122223333",
                PANNumber = "LOGIN1234L",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = "Chennai",
                Phone = "9000000000"
            };
            await _authService.RegisterAsync(registerDto);

            var loginDto = new LoginDTO
            {
                Email = "login@gmail.com",
                Password = "Login@123"
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Email, Is.EqualTo("login@gmail.com"));
        }

        // ── TEST 4: Login with Wrong Password Fails ───────────
        [Test]
        public async Task LoginAsync_WrongPassword_ThrowsException()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Name = "Wrong Pass Test",
                Email = "wrongpass@gmail.com",
                Password = "Correct@123",
                AadhaarNumber = "444455556666",
                PANNumber = "WRONG1234W",
                DateOfBirth = new DateTime(1992, 2, 2),
                Address = "Chennai",
                Phone = "9111111111"
            };
            await _authService.RegisterAsync(registerDto);

            var loginDto = new LoginDTO
            {
                Email = "wrongpass@gmail.com",
                Password = "Incorrect@123"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<PasswordException>(async () =>
                await _authService.LoginAsync(loginDto));

            Assert.That(ex!.Message, Does.Contain("incorrect"));
        }

        // ── TEST 5: Login with Non-Existent Email Fails ───────
        [Test]
        public async Task LoginAsync_NonExistentEmail_ThrowsException()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Email = "doesnotexist@gmail.com",
                Password = "Anything@123"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<TokenException>(async () =>
                await _authService.LoginAsync(loginDto));

            Assert.That(ex!.Message, Does.Contain("No account found"));
        }

        // ── TEST 6: Get Profile for Existing User ─────────────
        [Test]
        public async Task GetProfileAsync_ExistingUser_ReturnsCorrectProfile()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Name = "Profile Test User",
                Email = "profiletest@gmail.com",
                Password = "Profile@123",
                AadhaarNumber = "111111111111",
                PANNumber = "PROFI1234P",
                DateOfBirth = new DateTime(1995, 1, 1),
                Address = "Chennai",
                Phone = "9999999999"
            };
            await _authService.RegisterAsync(registerDto);
            var userId = await _context.Users
                .Where(u => u.Email == "profiletest@gmail.com")
                .Select(u => u.UserId)
                .FirstAsync();

            // Act
            var result = await _authService.GetProfileAsync(userId, "User");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Profile Test User"));
            Assert.That(result.Email, Is.EqualTo("profiletest@gmail.com"));
        }

        // ── TEST 7: Get Profile for Non-Existent User Fails ──
        [Test]
        public void GetProfileAsync_NonExistentUser_ThrowsException()
        {
            // Arrange
            int fakeUserId = 9999;

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _authService.GetProfileAsync(fakeUserId, "User"));

            Assert.That(ex!.Message, Is.EqualTo("Profile not found!"));
        }

        // ── TEST 8: Update Profile Successfully ───────────────
        [Test]
        public async Task UpdateProfileAsync_ValidData_UpdatesNameAndPhone()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Name = "Old Name",
                Email = "updatetest@gmail.com",
                Password = "Update@123",
                AadhaarNumber = "222222222222",
                PANNumber = "UPDAT1234U",
                DateOfBirth = new DateTime(1992, 3, 3),
                Address = "Chennai",
                Phone = "8888888888"
            };
            await _authService.RegisterAsync(registerDto);
            var userId = await _context.Users
                .Where(u => u.Email == "updatetest@gmail.com")
                .Select(u => u.UserId)
                .FirstAsync();

            var updateDto = new UpdateProfileDTO
            {
                Name = "New Updated Name",
                Phone = "7777777777"
            };

            // Act
            var result = await _authService.UpdateProfileAsync(userId, "User", updateDto);

            // Assert
            Assert.That(result.Name, Is.EqualTo("New Updated Name"));
            Assert.That(result.Phone, Is.EqualTo("7777777777"));
        }

        // ── TEST 9: Change Password Successfully ──────────────
        [Test]
        public async Task ChangePasswordAsync_CorrectCurrentPassword_ChangesSuccessfully()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Name = "Password Test",
                Email = "pwdtest@gmail.com",
                Password = "OldPass@123",
                AadhaarNumber = "333333333333",
                PANNumber = "PWDTE1234P",
                DateOfBirth = new DateTime(1990, 5, 5),
                Address = "Chennai",
                Phone = "6666666666"
            };
            await _authService.RegisterAsync(registerDto);
            var userId = await _context.Users
                .Where(u => u.Email == "pwdtest@gmail.com")
                .Select(u => u.UserId)
                .FirstAsync();

            var changeDto = new ChangePasswordDTO
            {
                CurrentPassword = "OldPass@123",
                NewPassword = "NewPass@456"
            };

            // Act
            var result = await _authService.ChangePasswordAsync(userId, "User", changeDto);

            // Assert
            Assert.That(result, Is.True);
        }

        // ── TEST 10: Change Password with Wrong Current Password
        [Test]
        public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsException()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Name = "Wrong Pwd Test",
                Email = "wrongpwdtest@gmail.com",
                Password = "Correct@123",
                AadhaarNumber = "444444444444",
                PANNumber = "WPWDT1234W",
                DateOfBirth = new DateTime(1988, 7, 7),
                Address = "Chennai",
                Phone = "5555555555"
            };
            await _authService.RegisterAsync(registerDto);
            var userId = await _context.Users
                .Where(u => u.Email == "wrongpwdtest@gmail.com")
                .Select(u => u.UserId)
                .FirstAsync();

            var changeDto = new ChangePasswordDTO
            {
                CurrentPassword = "WrongPassword@999",
                NewPassword = "NewPass@456"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<PasswordException>(async () =>
                await _authService.ChangePasswordAsync(userId, "User", changeDto));

            Assert.That(ex!.Message, Does.Contain("incorrect"));
        }
    }
}