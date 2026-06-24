namespace CrimeManagementSystem.DTOs
{
    public class OfficerResponseDTO
    {
        public int OfficerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BadgeNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ActiveIncidentsCount { get; set; }
    }
}