using CrimeManagementSystem.DTOs;
namespace CrimeManagementSystem.Interfaces
{
    public interface IOfficerService
    {
        Task<OfficerResponseDTO> AddOfficerAsync(AddOfficerDTO dto);
        Task<List<OfficerResponseDTO>> GetAllOfficersAsync();
        Task<OfficerResponseDTO> GetOfficerByIdAsync(int officerId);
        Task<bool> RemoveOfficerAsync(int officerId);
        Task<List<IncidentResponseDTO>> GetOfficerIncidentsAsync(int officerId);
    }
}