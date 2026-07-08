using CrimeManagementSystem.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CrimeManagementSystem.Tests
{
    [TestFixture]
    public class EmailServiceTests
    {
        private EmailService _emailService = null!;
        private Mock<ILogger<EmailService>> _loggerMock = null!;
        private IConfiguration _config = null!;

        [SetUp]
        public void Setup()
        {
            // Arrange (common setup)
            // Intentionally invalid SMTP server to test 
            // failure handling without sending real emails
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "EmailSettings:SmtpServer", "smtp.invalid-test-server.com" },
                { "EmailSettings:SmtpPort", "587" },
                { "EmailSettings:SenderEmail", "test@example.com" },
                { "EmailSettings:SenderPassword", "fake-password" },
                { "EmailSettings:SenderName", "Crime Management System" }
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _loggerMock = new Mock<ILogger<EmailService>>();
            _emailService = new EmailService(_config, _loggerMock.Object);
        }

        // ── TEST 1: SendEmailAsync Does Not Throw on Failure ──
        [Test]
        public void SendEmailAsync_InvalidSmtpServer_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "recipient@example.com";
            string subject = "Test Subject";
            string body = "<p>Test Body</p>";

            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 2: Welcome Email on Registration ─────────────
        [Test]
        public void SendEmailAsync_WelcomeEmail_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "nagashri@gmail.com";
            string subject = "Welcome to Crime Management System";
            string body = "<h2>Welcome, Nagashri!</h2>" +
                         "<p>Your account has been successfully created.</p>" +
                         "<p>You can now report incidents and track their status.</p>";

            // Act & Assert
            // Verifies welcome email (sent on user registration) 
            // doesn't crash the system
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 3: Login Notification Email ─────────────────
        [Test]
        public void SendEmailAsync_LoginNotification_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "nagashri@gmail.com";
            string subject = "Login Notification - Crime Management System";
            string body = "<p>Hello Nagashri,</p>" +
                         "<p>You have successfully logged in at 22-06-2026 10:30.</p>" +
                         "<p>If this wasn't you, please change your password immediately.</p>";

            // Act & Assert
            // Verifies login notification email (sent on every login) 
            // doesn't crash the system
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 4: Incident Creation Email with Incident ID ──
        [Test]
        public void SendEmailAsync_IncidentCreationEmail_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "nagashri@gmail.com";
            string subject = "Incident Registered Successfully - INC-2026-001";
            string body = "<h2>Your Incident Has Been Registered</h2>" +
                         "<p>Dear Nagashri,</p>" +
                         "<p>Your incident has been successfully registered.</p>" +
                         "<p><b>Incident ID:</b> INC-2026-001</p>" +
                         "<p><b>Type:</b> LostProperty</p>" +
                         "<p><b>Status:</b> Initiated</p>" +
                         "<p>Track your incident using ID: INC-2026-001</p>";

            // Act & Assert
            // Verifies incident creation email (sent when user 
            // reports a new incident) doesn't crash the system
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 5: Officer Assignment Email to User ──────────
        [Test]
        public void SendEmailAsync_OfficerAssignedEmail_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "nagashri@gmail.com";
            string subject = "Officer Assigned to Your Incident - INC-2026-001";
            string body = "<h2>An Officer Has Been Assigned</h2>" +
                         "<p>Dear Nagashri,</p>" +
                         "<p>Good news! An officer has been assigned to your incident.</p>" +
                         "<p><b>Incident ID:</b> INC-2026-001</p>" +
                         "<p><b>Assigned Officer:</b> Deepak</p>" +
                         "<p><b>Badge Number:</b> KC-001</p>" +
                         "<p><b>Status:</b> Active - Under Investigation</p>";

            // Act & Assert
            // Verifies officer assignment email (sent when Station Head 
            // assigns officer to incident) doesn't crash the system
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 6: Incident Closure Email to User ────────────
        [Test]
        public void SendEmailAsync_IncidentClosureEmail_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "nagashri@gmail.com";
            string subject = "Your Incident Has Been Closed - INC-2026-001";
            string body = "<h2>Incident Investigation Completed</h2>" +
                         "<p>Dear Nagashri,</p>" +
                         "<p>The investigation for your incident has been completed.</p>" +
                         "<p><b>Incident ID:</b> INC-2026-001</p>" +
                         "<p><b>Status:</b> Closed - Pending Verification</p>" +
                         "<p>You will be notified once the case is fully verified.</p>";

            // Act & Assert
            // Verifies closure email (sent when Officer closes 
            // the incident) doesn't crash the system
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 7: Incident Verified Email to User ───────────
        [Test]
        public void SendEmailAsync_IncidentVerifiedEmail_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "nagashri@gmail.com";
            string subject = "Your Incident Has Been Verified - INC-2026-001";
            string body = "<h2>Incident Successfully Verified ✅</h2>" +
                         "<p>Dear Nagashri,</p>" +
                         "<p>Your reported incident has been fully resolved and verified.</p>" +
                         "<p><b>Incident ID:</b> INC-2026-001</p>" +
                         "<p><b>Final Status:</b> Verified ✅</p>" +
                         "<p>Thank you for reporting. Your cooperation helps keep " +
                         "our community safe.</p>";

            // Act & Assert
            // Verifies verification email (sent when Station Head 
            // verifies the closed incident) doesn't crash the system
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 8: Officer Login Notification Email ──────────
        [Test]
        public void SendEmailAsync_OfficerLoginEmail_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "deepak.officer@cms.com";
            string subject = "Login Notification - Crime Management System";
            string body = "<p>Hello Officer Deepak,</p>" +
                         "<p>You have successfully logged in at 22-06-2026 10:30.</p>" +
                         "<p>If this wasn't you, please contact the " +
                         "Station Head immediately.</p>";

            // Act & Assert
            // Verifies officer login email (same as user login 
            // but with different message) doesn't crash
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 9: Email with HTML Content Doesn't Crash ─────
        [Test]
        public void SendEmailAsync_RichHtmlBody_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "recipient@example.com";
            string subject = "Rich HTML Email Test";
            string body = "<h1>Heading</h1>" +
                         "<table style='border-collapse:collapse;width:100%'>" +
                         "<tr><td style='padding:8px;border:1px solid #ddd'>" +
                         "<b>Field:</b></td>" +
                         "<td style='padding:8px;border:1px solid #ddd'>" +
                         "Value</td></tr>" +
                         "</table>" +
                         "<p><i>Footer text</i></p>";

            // Act & Assert
            // Verifies complex HTML email bodies 
            // (like our incident emails) don't crash
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 10: Error Logged When Email Fails ────────────
        [Test]
        public async Task SendEmailAsync_ConnectionFails_LogsErrorInternally()
        {
            // Arrange
            string toEmail = "recipient@example.com";
            string subject = "Test Subject";
            string body = "<p>Test Body</p>";

            // Act
            await _emailService.SendEmailAsync(toEmail, subject, body);

            // Assert
            // Verify error was logged (since fake SMTP server fails)
            // This confirms our try-catch logging works correctly
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}