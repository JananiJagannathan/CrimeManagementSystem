using CrimeManagementSystem.Enums;

namespace CrimeManagementSystem.DTOs
{
    public class CreateIncidentDTO
    {
        public IncidentType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string? PropertyDescription { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? SuspectDescription { get; set; }
        public string? GraffitiImagePath { get; set; }
    }
}