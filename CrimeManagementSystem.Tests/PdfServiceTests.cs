using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Interfaces;
using CrimeManagementSystem.Services;
using NUnit.Framework;

namespace CrimeManagementSystem.Tests
{
    [TestFixture]
    public class PdfServiceTests
    {
        private PdfService _pdfService = null!;

        [SetUp]
        public void Setup()
        {
            // Arrange (common setup)
            _pdfService = new PdfService();
        }

        // ── TEST 1: Generates Non-Empty PDF for Lost Property ──
        [Test]
        public void GenerateIncidentReport_LostPropertyIncident_ReturnsNonEmptyPdfBytes()
        {
            // Arrange
            var incident = new IncidentResponseDTO
            {
                IncidentId = 1,
                IncidentCode = "INC-2026-001",
                Type = "LostProperty",
                Status = "Initiated",
                Description = "Lost my laptop bag",
                Location = "Chennai Central",
                IncidentDate = DateTime.Now,
                ReportedAt = DateTime.Now,
                ReportedByName = "Nagashri",
                AssignedOfficerName = null,
                PropertyDescription = "Black laptop bag with charger"
            };

            // Act
            var result = _pdfService.GenerateIncidentReport(incident);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        // ── TEST 2: Generates PDF for Petit Larceny with Value ──
        [Test]
        public void GenerateIncidentReport_PetitLarcenyIncident_ReturnsNonEmptyPdfBytes()
        {
            // Arrange
            var incident = new IncidentResponseDTO
            {
                IncidentId = 2,
                IncidentCode = "INC-2026-002",
                Type = "PetitLarceny",
                Status = "Active",
                Description = "Bike stolen outside supermarket",
                Location = "T Nagar, Chennai",
                IncidentDate = DateTime.Now,
                ReportedAt = DateTime.Now,
                ReportedByName = "Haripriya",
                AssignedOfficerName = "Deepak",
                EstimatedValue = 850
            };

            // Act
            var result = _pdfService.GenerateIncidentReport(incident);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        // ── TEST 3: Generates PDF Even When Officer Not Assigned
        [Test]
        public void GenerateIncidentReport_NoOfficerAssigned_HandlesGracefully()
        {
            // Arrange
            var incident = new IncidentResponseDTO
            {
                IncidentId = 3,
                IncidentCode = "INC-2026-003",
                Type = "Graffiti",
                Status = "Initiated",
                Description = "Graffiti on temple wall",
                Location = "Mylapore, Chennai",
                IncidentDate = DateTime.Now,
                ReportedAt = DateTime.Now,
                ReportedByName = "Nasiya",
                AssignedOfficerName = null,  // No officer assigned yet
                GraffitiImagePath = "graffiti_001.jpg"
            };

            // Act
            var result = _pdfService.GenerateIncidentReport(incident);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        // ── TEST 4: Generates PDF for Criminal Mischief Type ──
        [Test]
        public void GenerateIncidentReport_CriminalMischiefIncident_ReturnsNonEmptyPdfBytes()
        {
            // Arrange
            var incident = new IncidentResponseDTO
            {
                IncidentId = 4,
                IncidentCode = "INC-2026-004",
                Type = "CriminalMischief",
                Status = "Verified",
                Description = "Car side mirror intentionally broken",
                Location = "Anna Nagar, Chennai",
                IncidentDate = DateTime.Now,
                ReportedAt = DateTime.Now,
                ReportedByName = "Iswariya",
                AssignedOfficerName = "Suresh",
                SuspectDescription = "Tall man wearing a red cap"
            };

            // Act
            var result = _pdfService.GenerateIncidentReport(incident);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        // ── TEST 5: PDF Output Starts with Valid PDF Header Bytes
        [Test]
        public void GenerateIncidentReport_ValidIncident_ProducesValidPdfFormat()
        {
            // Arrange
            var incident = new IncidentResponseDTO
            {
                IncidentId = 5,
                IncidentCode = "INC-2026-005",
                Type = "LostProperty",
                Status = "Closed",
                Description = "Lost mobile phone",
                Location = "Adyar Bus Stand",
                IncidentDate = DateTime.Now,
                ReportedAt = DateTime.Now,
                ReportedByName = "Lakshmi Priya",
                AssignedOfficerName = "Kavitha"
            };

            // Act
            var result = _pdfService.GenerateIncidentReport(incident);

            // Assert
            // Valid PDF files start with the bytes "%PDF" (0x25 0x50 0x44 0x46)
            Assert.That(result[0], Is.EqualTo(0x25)); // %
            Assert.That(result[1], Is.EqualTo(0x50)); // P
            Assert.That(result[2], Is.EqualTo(0x44)); // D
            Assert.That(result[3], Is.EqualTo(0x46)); // F
        }
    }
}