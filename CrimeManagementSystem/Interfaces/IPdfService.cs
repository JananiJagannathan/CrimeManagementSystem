using CrimeManagementSystem.DTOs;

namespace CrimeManagementSystem.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateIncidentReport(IncidentResponseDTO incident);
    }
}