using CrimeManagementSystem.Enums;

namespace CrimeManagementSystem.Models
{
    public class Officer
    {
        public int OfficerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string BadgeNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Incident> AssignedIncidents { get; set; } = new List<Incident>();
    }
}