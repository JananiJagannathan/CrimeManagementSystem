using CrimeManagementSystem.Enums;

namespace CrimeManagementSystem.Models
{
    public class Incident
    {
        public int IncidentId { get; set; }
        public string IncidentCode { get; set; } = string.Empty;
        public IncidentType Type { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Initiated;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.Now;
        public string? PropertyDescription { get; set; }   
        public decimal? EstimatedValue { get; set; }       
        public string? SuspectDescription { get; set; }    
        public string? GraffitiImagePath { get; set; }    
        public int ReportedByUserId { get; set; }
        public int? AssignedOfficerId { get; set; }
        public User ReportedBy { get; set; } = null!;
        public Officer? AssignedOfficer { get; set; }
    }
}