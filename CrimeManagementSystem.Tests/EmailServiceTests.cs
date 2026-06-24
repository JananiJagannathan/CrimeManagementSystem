using CrimeManagementSystem.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

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

            // Fake email configuration with INVALID server 
            // (intentionally, so we test failure handling 
            // without actually sending real emails)
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "EmailSettings:SmtpServer", "smtp.invalid-test-server.com" },
                { "EmailSettings:SmtpPort", "587" },
                { "EmailSettings:SenderEmail", "test@example.com" },
                { "EmailSettings:SenderPassword", "fake-password" },
                { "EmailSettings:SenderName", "Test Sender" }
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _loggerMock = new Mock<ILogger<EmailService>>();

            _emailService = new EmailService(_config, _loggerMock.Object);
        }

        // ── TEST 1: SendEmailAsync Does Not Throw Even on Failure
        [Test]
        public void SendEmailAsync_InvalidSmtpServer_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "recipient@example.com";
            string subject = "Test Subject";
            string body = "<p>Test Body</p>";

            // Act & Assert
            // Our design ensures email failures are caught and 
            // logged internally, never crashing the calling code
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 2: SendEmailAsync Completes Even With Empty Body
        [Test]
        public void SendEmailAsync_EmptyBody_DoesNotThrowException()
        {
            // Arrange
            string toEmail = "recipient@example.com";
            string subject = "Test Subject";
            string body = "";

            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
                await _emailService.SendEmailAsync(toEmail, subject, body));
        }

        // ── TEST 3: SendEmailAsync Logs Error When Sending Fails
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
            // Verify that an error was logged at least once 
            // (since our fake SMTP server will fail to connect)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        // ── TEST 4: Multiple Calls Don't Throw or Crash
        [Test]
        public void SendEmailAsync_MultipleCalls_AllCompleteWithoutThrowing()
        {
            // Arrange
            var recipients = new[] { "user1@test.com", "user2@test.com", "user3@test.com" };

            // Act & Assert
            foreach (var recipient in recipients)
            {
                Assert.DoesNotThrowAsync(async () =>
                    await _emailService.SendEmailAsync(recipient, "Subject", "Body"));
            }
        }
    }
}