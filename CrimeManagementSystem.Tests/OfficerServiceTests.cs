using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Enums;
using CrimeManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CrimeManagementSystem.Tests
{
    [TestFixture]
    public class OfficerServiceTests
    {
        private AppDbContext _context = null!;
        private OfficerService _officerService = null!;
        private Mock<ILogger<OfficerService>> _loggerMock = null!;

        [SetUp]
        public void Setup()
        {
            // Arrange (common setup)
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            _loggerMock = new Mock<ILogger<OfficerService>>();

            _officerService = new OfficerService(_context, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ── TEST 1: Add Officer Successfully ──────────────────
        [Test]
        public async Task AddOfficerAsync_ValidData_ReturnsOfficerWithActiveStatus()
        {
            // Arrange
            var dto = new AddOfficerDTO
            {
                Name = "Suresh Babu",
                Email = "suresh@cms.com",
                Password = "Officer@123",
                BadgeNumber = "TN-001",
                Phone = "9444000004",
                Address = "Adyar, Chennai"
            };

            // Act
            var result = await _officerService.AddOfficerAsync(dto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Suresh Babu"));
            Assert.That(result.BadgeNumber, Is.EqualTo("TN-001"));
            Assert.That(result.IsActive, Is.True);
            Assert.That(result.ActiveIncidentsCount, Is.EqualTo(0));
        }

        // ── TEST 2: Duplicate Email Fails ─────────────────────
        [Test]
        public async Task AddOfficerAsync_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var dto = new AddOfficerDTO
            {
                Name = "Officer One",
                Email = "duplicate@cms.com",
                Password = "Officer@123",
                BadgeNumber = "TN-001",
                Phone = "9444000004",
                Address = "Chennai"
            };
            await _officerService.AddOfficerAsync(dto);

            var dto2 = new AddOfficerDTO
            {
                Name = "Officer Two",
                Email = "duplicate@cms.com", // same email
                Password = "Officer@456",
                BadgeNumber = "TN-002",
                Phone = "9444000005",
                Address = "Chennai"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _officerService.AddOfficerAsync(dto2));

            Assert.That(ex!.Message, Is.EqualTo("Officer with this email already exists!"));
        }

        // ── TEST 3: Duplicate Badge Number Fails ──────────────
        [Test]
        public async Task AddOfficerAsync_DuplicateBadgeNumber_ThrowsException()
        {
            // Arrange
            var dto = new AddOfficerDTO
            {
                Name = "Officer One",
                Email = "officer1@cms.com",
                Password = "Officer@123",
                BadgeNumber = "TN-999",
                Phone = "9444000004",
                Address = "Chennai"
            };
            await _officerService.AddOfficerAsync(dto);

            var dto2 = new AddOfficerDTO
            {
                Name = "Officer Two",
                Email = "officer2@cms.com",
                Password = "Officer@456",
                BadgeNumber = "TN-999", // same badge
                Phone = "9444000005",
                Address = "Chennai"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _officerService.AddOfficerAsync(dto2));

            Assert.That(ex!.Message, Is.EqualTo("Badge number already exists!"));
        }

        // ── TEST 4: Get All Officers Returns List ─────────────
        [Test]
        public async Task GetAllOfficersAsync_MultipleOfficers_ReturnsAllWithCorrectCount()
        {
            // Arrange
            await _officerService.AddOfficerAsync(new AddOfficerDTO
            {
                Name = "Officer A",
                Email = "officerA@cms.com",
                Password = "Pass@123",
                BadgeNumber = "TN-100",
                Phone = "9111111111",
                Address = "Chennai"
            });
            await _officerService.AddOfficerAsync(new AddOfficerDTO
            {
                Name = "Officer B",
                Email = "officerB@cms.com",
                Password = "Pass@123",
                BadgeNumber = "TN-101",
                Phone = "9222222222",
                Address = "Chennai"
            });

            // Act
            var result = await _officerService.GetAllOfficersAsync();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // ── TEST 5: Remove Officer with No Active Incidents ───
        [Test]
        public async Task RemoveOfficerAsync_NoActiveIncidents_DeactivatesOfficer()
        {
            // Arrange
            var officer = await _officerService.AddOfficerAsync(new AddOfficerDTO
            {
                Name = "Officer To Remove",
                Email = "remove@cms.com",
                Password = "Pass@123",
                BadgeNumber = "TN-200",
                Phone = "9333333333",
                Address = "Chennai"
            });

            // Act
            var result = await _officerService.RemoveOfficerAsync(officer.OfficerId);

            // Assert
            Assert.That(result, Is.True);

            var updatedOfficer = await _context.Officers.FindAsync(officer.OfficerId);
            Assert.That(updatedOfficer!.IsActive, Is.False);
        }

        // ── TEST 6: Remove Non-Existent Officer Fails ─────────
        [Test]
        public async Task RemoveOfficerAsync_OfficerNotFound_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _officerService.RemoveOfficerAsync(9999));

            Assert.That(ex!.Message, Is.EqualTo("Officer not found!"));
        }
        // ── TEST 7: Get Officer By Id Successfully ────────────
        [Test]
        public async Task GetOfficerByIdAsync_ExistingOfficer_ReturnsCorrectOfficer()
        {
            // Arrange
            var addedOfficer = await _officerService.AddOfficerAsync(new AddOfficerDTO
            {
                Name = "Lookup Officer",
                Email = "lookup@cms.com",
                Password = "Pass@123",
                BadgeNumber = "TN-300",
                Phone = "9444444444",
                Address = "Chennai"
            });

            // Act
            var result = await _officerService.GetOfficerByIdAsync(addedOfficer.OfficerId);

            // Assert
            Assert.That(result.Name, Is.EqualTo("Lookup Officer"));
            Assert.That(result.BadgeNumber, Is.EqualTo("TN-300"));
        }

        // ── TEST 8: Get Officer By Id for Non-Existent Officer
        [Test]
        public void GetOfficerByIdAsync_NonExistentOfficer_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _officerService.GetOfficerByIdAsync(9999));

            Assert.That(ex!.Message, Is.EqualTo("Officer not found!"));
        }

        // ── TEST 9: Get Officer Incidents Returns Empty List When None Assigned
        [Test]
        public async Task GetOfficerIncidentsAsync_NoIncidentsAssigned_ReturnsEmptyList()
        {
            // Arrange
            var officer = await _officerService.AddOfficerAsync(new AddOfficerDTO
            {
                Name = "Idle Officer",
                Email = "idle@cms.com",
                Password = "Pass@123",
                BadgeNumber = "TN-400",
                Phone = "9555555555",
                Address = "Chennai"
            });

            // Act
            var result = await _officerService.GetOfficerIncidentsAsync(officer.OfficerId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // ── TEST 10: Remove Already Removed Officer Behavior ──
        [Test]
        public async Task RemoveOfficerAsync_AlreadyDeactivatedOfficer_StillSucceeds()
        {
            // Arrange
            var officer = await _officerService.AddOfficerAsync(new AddOfficerDTO
            {
                Name = "Double Remove Officer",
                Email = "doubleremove@cms.com",
                Password = "Pass@123",
                BadgeNumber = "TN-500",
                Phone = "9666666666",
                Address = "Chennai"
            });

            await _officerService.RemoveOfficerAsync(officer.OfficerId); // first removal

            // Act
            var result = await _officerService.RemoveOfficerAsync(officer.OfficerId); // second removal

            // Assert
            Assert.That(result, Is.True);
        }
    }
}