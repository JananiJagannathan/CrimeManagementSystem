using NUnit.Framework;
using QuestPDF.Infrastructure;

namespace CrimeManagementSystem.Tests
{
    [SetUpFixture]
    public class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
    }
}