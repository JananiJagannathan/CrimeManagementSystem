using CrimeManagementSystem.DTOs;
namespace CrimeManagementSystem.Interfaces
{
    public interface IIncidentService
    {
        Task<IncidentResponseDTO> CreateIncidentAsync(CreateIncidentDTO dto, int userId);
        Task<List<IncidentResponseDTO>> GetUserIncidentsAsync(int userId);
        Task<IncidentResponseDTO> GetIncidentByIdAsync(int incidentId);
        Task<List<IncidentResponseDTO>> GetAllIncidentsAsync();
        Task<IncidentResponseDTO> AssignOfficerAsync(AssignOfficerDTO dto);
        Task<IncidentResponseDTO> CloseIncidentAsync(int incidentId, int officerId);
        Task<IncidentResponseDTO> VerifyIncidentAsync(int incidentId);
    }
}