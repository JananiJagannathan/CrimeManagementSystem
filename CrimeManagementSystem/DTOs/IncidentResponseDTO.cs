using CrimeManagementSystem.Enums;

namespace CrimeManagementSystem.DTOs
{
    public class IncidentResponseDTO
    {
        public int IncidentId { get; set; }
        public string IncidentCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public DateTime ReportedAt { get; set; }
        public string ReportedByName { get; set; } = string.Empty;
        public string? AssignedOfficerName { get; set; }
        public string? PropertyDescription { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? SuspectDescription { get; set; }
        public string? GraffitiImagePath { get; set; }
    }
}