using AutoMapper;
using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Enums;
using CrimeManagementSystem.Exceptions;
using CrimeManagementSystem.Interfaces;
using CrimeManagementSystem.Mappings;
using CrimeManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CrimeManagementSystem.Tests
{
    [TestFixture]
    public class IncidentServiceTests
    {
        private AppDbContext _context = null!;
        private IncidentService _incidentService = null!;
        private Mock<ILogger<IncidentService>> _loggerMock = null!;
        private IMapper _mapper = null!;
        private Mock<IEmailService> _emailServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _loggerMock = new Mock<ILogger<IncidentService>>();
            _emailServiceMock = new Mock<IEmailService>();

            _incidentService = new IncidentService(
                _context, _mapper, _loggerMock.Object, _emailServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // Helper to create a test user in DB
        private async Task<int> CreateTestUserAsync()
        {
            var user = new Models.User
            {
                Name = "Test Reporter",
                Email = "reporter@test.com",
                PasswordHash = "hashedpassword",
                AadhaarNumber = "123456789012",
                PANNumber = "ABCDE1234F",
                DateOfBirth = new DateTime(1995, 1, 1),
                Address = "Test Address",
                Phone = "9999999999",
                Role = UserRole.User
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.UserId;
        }

        // Helper to create a test officer in DB
        private async Task<int> CreateTestOfficerAsync()
        {
            var officer = new Models.Officer
            {
                Name = "Test Officer",
                Email = "officer@test.com",
                PasswordHash = "hashedpassword",
                BadgeNumber = "TEST-001",
                Phone = "8888888888",
                Address = "Test Station",
                IsActive = true
            };
            _context.Officers.Add(officer);
            await _context.SaveChangesAsync();
            return officer.OfficerId;
        }

        // ── TEST 1: Create Incident Successfully ──────────────
        [Test]
        public async Task CreateIncidentAsync_ValidData_ReturnsIncidentWithInitiatedStatus()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var dto = new CreateIncidentDTO
            {
                Type = IncidentType.LostProperty,
                Description = "Lost my wallet",
                Location = "Chennai Central",
                IncidentDate = DateTime.Now,
                PropertyDescription = "Brown leather wallet"
            };

            // Act
            var result = await _incidentService.CreateIncidentAsync(dto, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo("Initiated"));
            Assert.That(result.IncidentCode, Does.StartWith("INC-"));
            Assert.That(result.ReportedByName, Is.EqualTo("Test Reporter"));
        }

        // ── TEST 2: Get User's Own Incidents ──────────────────
        [Test]
        public async Task GetUserIncidentsAsync_UserHasIncidents_ReturnsOnlyTheirIncidents()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var dto = new CreateIncidentDTO
            {
                Type = IncidentType.PetitLarceny,
                Description = "Bike stolen",
                Location = "T Nagar",
                IncidentDate = DateTime.Now,
                EstimatedValue = 5000
            };
            await _incidentService.CreateIncidentAsync(dto, userId);

            // Act
            var result = await _incidentService.GetUserIncidentsAsync(userId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Type, Is.EqualTo("PetitLarceny"));
        }

        // ── TEST 3: Assign Officer Changes Status to Active ───
        [Test]
        public async Task AssignOfficerAsync_ValidIds_ChangesStatusToActive()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var officerId = await CreateTestOfficerAsync();
            var dto = new CreateIncidentDTO
            {
                Type = IncidentType.CriminalMischief,
                Description = "Broken window",
                Location = "Adyar",
                IncidentDate = DateTime.Now
            };
            var incident = await _incidentService.CreateIncidentAsync(dto, userId);

            var assignDto = new AssignOfficerDTO
            {
                IncidentId = incident.IncidentId,
                OfficerId = officerId
            };

            // Act
            var result = await _incidentService.AssignOfficerAsync(assignDto);

            // Assert
            Assert.That(result.Status, Is.EqualTo("Active"));
            Assert.That(result.AssignedOfficerName, Is.EqualTo("Test Officer"));
        }

        // ── TEST 4: Assign Officer with Invalid Incident Fails ─
        [Test]
        public void AssignOfficerAsync_InvalidIncidentId_ThrowsException()
        {
            // Arrange
            var assignDto = new AssignOfficerDTO
            {
                IncidentId = 9999,
                OfficerId = 1
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _incidentService.AssignOfficerAsync(assignDto));

            Assert.That(ex!.Message, Does.Contain("was not found in the system"));
        }

        // ── TEST 5: Close Incident Changes Status to Closed ───
        [Test]
        public async Task CloseIncidentAsync_AssignedOfficer_ChangesStatusToClosed()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var officerId = await CreateTestOfficerAsync();
            var dto = new CreateIncidentDTO
            {
                Type = IncidentType.Graffiti,
                Description = "Wall painted",
                Location = "Mylapore",
                IncidentDate = DateTime.Now,
                GraffitiImagePath = "test.jpg"
            };
            var incident = await _incidentService.CreateIncidentAsync(dto, userId);
            await _incidentService.AssignOfficerAsync(new AssignOfficerDTO
            {
                IncidentId = incident.IncidentId,
                OfficerId = officerId
            });

            // Act
            var result = await _incidentService.CloseIncidentAsync(incident.IncidentId, officerId);

            // Assert
            Assert.That(result.Status, Is.EqualTo("Closed"));
        }

        // ── TEST 6: Verify Incident Requires Closed Status First
        [Test]
        public async Task VerifyIncidentAsync_NotClosedYet_ThrowsException()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var dto = new CreateIncidentDTO
            {
                Type = IncidentType.LostProperty,
                Description = "Lost phone",
                Location = "Velachery",
                IncidentDate = DateTime.Now
            };
            var incident = await _incidentService.CreateIncidentAsync(dto, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<IncidentAssignmentException>(async () =>
                await _incidentService.VerifyIncidentAsync(incident.IncidentId));

            Assert.That(ex!.Message, Does.Contain("must be 'Closed'"));
        }

        // ── TEST 7: Get Incident By Id for Non-Existent Incident
        [Test]
        public void GetIncidentByIdAsync_NonExistentId_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _incidentService.GetIncidentByIdAsync(9999));

            Assert.That(ex!.Message, Does.Contain("was not found in the system"));
        }

        // ── TEST 8: Get All Incidents Returns Correct Count ───
        [Test]
        public async Task GetAllIncidentsAsync_MultipleIncidents_ReturnsAllOfThem()
        {
            // Arrange
            var userId = await CreateTestUserAsync();

            await _incidentService.CreateIncidentAsync(new CreateIncidentDTO
            {
                Type = IncidentType.LostProperty,
                Description = "First incident description",
                Location = "Chennai",
                IncidentDate = DateTime.Now,
                PropertyDescription = "Bag"
            }, userId);

            await _incidentService.CreateIncidentAsync(new CreateIncidentDTO
            {
                Type = IncidentType.PetitLarceny,
                Description = "Second incident description",
                Location = "Chennai",
                IncidentDate = DateTime.Now,
                EstimatedValue = 500
            }, userId);

            // Act
            var result = await _incidentService.GetAllIncidentsAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // ── TEST 9: Close Incident Not Assigned to Officer Fails
        [Test]
        public async Task CloseIncidentAsync_NotAssignedToOfficer_ThrowsException()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var officerId = await CreateTestOfficerAsync();

            var incident = await _incidentService.CreateIncidentAsync(new CreateIncidentDTO
            {
                Type = IncidentType.LostProperty,
                Description = "Unassigned incident",
                Location = "Chennai",
                IncidentDate = DateTime.Now,
                PropertyDescription = "Phone"
            }, userId);

            // Act & Assert
            var ex = Assert.ThrowsAsync<IncidentAssignmentException>(async () =>
                await _incidentService.CloseIncidentAsync(incident.IncidentId, officerId));

            Assert.That(ex!.Message, Does.Contain("not assigned to you"));
        }

        // ── TEST 10: Verify Incident That Doesn't Exist Fails ─
        [Test]
        public void VerifyIncidentAsync_NonExistentIncident_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _incidentService.VerifyIncidentAsync(9999));

            Assert.That(ex!.Message, Does.Contain("was not found in the system"));
        }

        // ── TEST 11: Create Incident with Petit Larceny Type ──
        [Test]
        public async Task CreateIncidentAsync_PetitLarcenyType_StoresEstimatedValueCorrectly()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var dto = new CreateIncidentDTO
            {
                Type = IncidentType.PetitLarceny,
                Description = "Bike stolen from parking",
                Location = "Chennai",
                IncidentDate = DateTime.Now,
                EstimatedValue = 750
            };

            // Act
            var result = await _incidentService.CreateIncidentAsync(dto, userId);

            // Assert
            Assert.That(result.Type, Is.EqualTo("PetitLarceny"));
            Assert.That(result.EstimatedValue, Is.EqualTo(750));
        }
    }
}