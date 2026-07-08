using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Enums;
using CrimeManagementSystem.Interfaces;
using CrimeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CrimeManagementSystem.Services
{
    public class OfficerService : IOfficerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OfficerService> _logger;

        public OfficerService(AppDbContext context, ILogger<OfficerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── ADD OFFICER (Station Head) ────────────────────────
        public async Task<OfficerResponseDTO> AddOfficerAsync(AddOfficerDTO dto)
        {
            try
            {
                var existing = await _context.Officers
                    .FirstOrDefaultAsync(o => o.Email == dto.Email);

                if (existing != null)
                {
                    _logger.LogWarning("Duplicate officer email attempt: {Email}", dto.Email);
                    throw new Exceptions.DuplicateResourceException(
                        $"An officer with email '{dto.Email}' already exists in the system.");
                }

                var existingBadge = await _context.Officers
                    .FirstOrDefaultAsync(o => o.BadgeNumber == dto.BadgeNumber);

                if (existingBadge != null)
                {
                    _logger.LogWarning("Duplicate badge number attempt: {Badge}", dto.BadgeNumber);
                    throw new Exceptions.DuplicateResourceException(
                        $"An officer with badge number '{dto.BadgeNumber}' already exists in the system.");
                }

                var officer = new Officer
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    BadgeNumber = dto.BadgeNumber,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Officers.Add(officer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New officer added: {OfficerId} - {Badge}",
                    officer.OfficerId, officer.BadgeNumber);

                return MapToDTO(officer, 0);
            }
            catch (Exception ex) when (ex is not Exceptions.DuplicateResourceException)
            {
                _logger.LogError(ex, "Error adding officer with email {Email}", dto.Email);
                throw new Exception("Failed to add officer. Please try again later.");
            }
        }

        // ── GET ALL OFFICERS ──────────────────────────────────
        public async Task<List<OfficerResponseDTO>> GetAllOfficersAsync()
        {
            try
            {
                var officers = await _context.Officers.ToListAsync();
                var result = new List<OfficerResponseDTO>();

                foreach (var officer in officers)
                {
                    var activeCount = await _context.Incidents
                        .CountAsync(i => i.AssignedOfficerId == officer.OfficerId
                                   && i.Status == IncidentStatus.Active);

                    result.Add(MapToDTO(officer, activeCount));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all officers");
                throw new Exception("Failed to fetch officers. Please try again later.");
            }
        }

        // ── GET OFFICER BY ID ─────────────────────────────────
        public async Task<OfficerResponseDTO> GetOfficerByIdAsync(int officerId)
        {
            try
            {
                var officer = await _context.Officers
                    .FirstOrDefaultAsync(o => o.OfficerId == officerId);

                if (officer == null)
                {
                    _logger.LogWarning("Officer not found: {OfficerId}", officerId);
                    throw new Exceptions.NotFoundException(
                        $"Officer with ID {officerId} was not found in the system.");
                }

                var activeCount = await _context.Incidents
                    .CountAsync(i => i.AssignedOfficerId == officerId
                               && i.Status == IncidentStatus.Active);

                return MapToDTO(officer, activeCount);
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error fetching officer {OfficerId}", officerId);
                throw new Exception("Failed to fetch officer. Please try again later.");
            }
        }

        // ── REMOVE OFFICER ────────────────────────────────────
        public async Task<bool> RemoveOfficerAsync(int officerId)
        {
            try
            {
                var officer = await _context.Officers
                    .FirstOrDefaultAsync(o => o.OfficerId == officerId);

                if (officer == null)
                {
                    _logger.LogWarning("Remove failed - Officer not found: {OfficerId}", officerId);
                    throw new Exceptions.NotFoundException(
                        $"Officer with ID {officerId} was not found in the system.");
                }

                var activeIncidents = await _context.Incidents
                    .CountAsync(i => i.AssignedOfficerId == officerId
                               && i.Status == IncidentStatus.Active);

                if (activeIncidents > 0)
                {
                    _logger.LogWarning("Remove blocked - Officer {OfficerId} has {Count} active incidents",
                        officerId, activeIncidents);
                    throw new Exceptions.IncidentAssignmentException(
                        $"Officer with ID {officerId} cannot be removed because they currently " +
                        $"have {activeIncidents} active incident(s) assigned. " +
                        $"Please reassign or close these incidents first.");
                }

                officer.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Officer removed (deactivated): {OfficerId}", officerId);

                return true;
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException
                                    && ex is not Exceptions.IncidentAssignmentException)
            {
                _logger.LogError(ex, "Error removing officer {OfficerId}", officerId);
                throw new Exception("Failed to remove officer. Please try again later.");
            }
        }

        // ── REACTIVATE OFFICER ────────────────────────────────
        public async Task<OfficerResponseDTO> ReactivateOfficerAsync(int officerId)
        {
            try
            {
                var officer = await _context.Officers
                    .FirstOrDefaultAsync(o => o.OfficerId == officerId);

                if (officer == null)
                {
                    _logger.LogWarning("Reactivate failed - Officer not found: {OfficerId}", officerId);
                    throw new Exceptions.NotFoundException(
                        $"Officer with ID {officerId} was not found in the system.");
                }

                if (officer.IsActive)
                {
                    _logger.LogWarning("Reactivate skipped - Officer {OfficerId} already active",
                        officerId);
                    throw new Exceptions.IncidentAssignmentException(
                        $"Officer with ID {officerId} is already active " +
                        $"and does not need reactivation.");
                }

                officer.IsActive = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Officer reactivated: {OfficerId}", officerId);

                var activeCount = await _context.Incidents
                    .CountAsync(i => i.AssignedOfficerId == officerId
                               && i.Status == IncidentStatus.Active);

                return MapToDTO(officer, activeCount);
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException
                                    && ex is not Exceptions.IncidentAssignmentException)
            {
                _logger.LogError(ex, "Error reactivating officer {OfficerId}", officerId);
                throw new Exception("Failed to reactivate officer. Please try again later.");
            }
        }

        // ── GET OFFICER INCIDENTS ─────────────────────────────
        public async Task<List<IncidentResponseDTO>> GetOfficerIncidentsAsync(int officerId)
        {
            try
            {
                var incidents = await _context.Incidents
                    .Include(i => i.ReportedBy)
                    .Include(i => i.AssignedOfficer)
                    .Where(i => i.AssignedOfficerId == officerId)
                    .ToListAsync();

                return incidents.Select(i => new IncidentResponseDTO
                {
                    IncidentId = i.IncidentId,
                    IncidentCode = i.IncidentCode,
                    Type = i.Type.ToString(),
                    Status = i.Status.ToString(),
                    Description = i.Description,
                    Location = i.Location,
                    IncidentDate = i.IncidentDate,
                    ReportedAt = i.ReportedAt,
                    ReportedByName = i.ReportedBy?.Name ?? "",
                    AssignedOfficerName = i.AssignedOfficer?.Name,
                    PropertyDescription = i.PropertyDescription,
                    EstimatedValue = i.EstimatedValue,
                    SuspectDescription = i.SuspectDescription,
                    GraffitiImagePath = i.GraffitiImagePath
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching incidents for officer {OfficerId}", officerId);
                throw new Exception("Failed to fetch officer incidents. Please try again later.");
            }
        }

        // ── MAP TO DTO ────────────────────────────────────────
        private OfficerResponseDTO MapToDTO(Officer officer, int activeCount)
        {
            return new OfficerResponseDTO
            {
                OfficerId = officer.OfficerId,
                Name = officer.Name,
                Email = officer.Email,
                BadgeNumber = officer.BadgeNumber,
                Phone = officer.Phone,
                IsActive = officer.IsActive,
                ActiveIncidentsCount = activeCount
            };
        }
    }
}