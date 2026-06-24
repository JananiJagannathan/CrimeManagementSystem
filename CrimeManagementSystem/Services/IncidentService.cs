using AutoMapper;
using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Enums;
using CrimeManagementSystem.Interfaces;
using CrimeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CrimeManagementSystem.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<IncidentService> _logger;

        public IncidentService(AppDbContext context, IMapper mapper, ILogger<IncidentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        //  CREATE INCIDENT 
        public async Task<IncidentResponseDTO> CreateIncidentAsync(CreateIncidentDTO dto, int userId)
        {
            try
            {
                var count = await _context.Incidents.CountAsync();
                var incidentCode = $"INC-{DateTime.Now.Year}-{(count + 1):D3}";

                var incident = new Incident
                {
                    IncidentCode = incidentCode,
                    Type = dto.Type,
                    Status = IncidentStatus.Initiated,
                    Description = dto.Description,
                    Location = dto.Location,
                    IncidentDate = dto.IncidentDate,
                    ReportedAt = DateTime.Now,
                    ReportedByUserId = userId,
                    PropertyDescription = dto.PropertyDescription,
                    EstimatedValue = dto.EstimatedValue,
                    SuspectDescription = dto.SuspectDescription,
                    GraffitiImagePath = dto.GraffitiImagePath
                };

                _context.Incidents.Add(incident);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident created: {Code} by UserId {UserId}", incidentCode, userId);

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incident for UserId {UserId}", userId);
                throw new Exception("Failed to create incident. Please try again later.");
            }
        }

        // GET USER INCIDENTS 
        public async Task<List<IncidentResponseDTO>> GetUserIncidentsAsync(int userId)
        {
            try
            {
                var incidents = await _context.Incidents
                    .Include(i => i.ReportedBy)
                    .Include(i => i.AssignedOfficer)
                    .Where(i => i.ReportedByUserId == userId)
                    .ToListAsync();

                return incidents.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching incidents for UserId {UserId}", userId);
                throw new Exception("Failed to fetch incidents. Please try again later.");
            }
        }

        //  GET INCIDENT BY ID  (CUSTOM EXCEPTION => NotFoundException)
        public async Task<IncidentResponseDTO> GetIncidentByIdAsync(int incidentId)
        {
            try
            {
                var incident = await _context.Incidents
                    .Include(i => i.ReportedBy)
                    .Include(i => i.AssignedOfficer)
                    .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

                if (incident == null)
                {
                    _logger.LogWarning("Incident not found: {IncidentId}", incidentId);
                    throw new Exceptions.NotFoundException("Incident not found!");
                }

                return MapToDTO(incident);
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error fetching incident {IncidentId}", incidentId);
                throw new Exception("Failed to fetch incident. Please try again later.");
            }
        }

        // GET ALL INCIDENTS (Station Head)
        public async Task<List<IncidentResponseDTO>> GetAllIncidentsAsync()
        {
            try
            {
                var incidents = await _context.Incidents
                    .Include(i => i.ReportedBy)
                    .Include(i => i.AssignedOfficer)
                    .ToListAsync();

                return incidents.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all incidents");
                throw new Exception("Failed to fetch incidents. Please try again later.");
            }
        }

        //  ASSIGN OFFICER 
        public async Task<IncidentResponseDTO> AssignOfficerAsync(AssignOfficerDTO dto)
        {
            try
            {
                var incident = await _context.Incidents
                    .FirstOrDefaultAsync(i => i.IncidentId == dto.IncidentId);

                if (incident == null)
                {
                    _logger.LogWarning("Assign failed - Incident not found: {IncidentId}", dto.IncidentId);
                    throw new Exception("Incident not found!");
                }

                var officer = await _context.Officers
                    .FirstOrDefaultAsync(o => o.OfficerId == dto.OfficerId);

                if (officer == null)
                {
                    _logger.LogWarning("Assign failed - Officer not found: {OfficerId}", dto.OfficerId);
                    throw new Exception("Officer not found!");
                }

                incident.AssignedOfficerId = dto.OfficerId;
                incident.Status = IncidentStatus.Active;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident {IncidentId} assigned to Officer {OfficerId}",
                    dto.IncidentId, dto.OfficerId);

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex) when (ex.Message is not ("Incident not found!" or "Officer not found!"))
            {
                _logger.LogError(ex, "Error assigning officer to incident {IncidentId}", dto.IncidentId);
                throw new Exception("Failed to assign officer. Please try again later.");
            }
        }

        // CLOSE INCIDENT (Officer) 
        public async Task<IncidentResponseDTO> CloseIncidentAsync(int incidentId, int officerId)
        {
            try
            {
                var incident = await _context.Incidents
                    .FirstOrDefaultAsync(i => i.IncidentId == incidentId
                                        && i.AssignedOfficerId == officerId);

                if (incident == null)
                {
                    _logger.LogWarning("Close failed - Incident {IncidentId} not assigned to Officer {OfficerId}",
                        incidentId, officerId);
                    throw new Exception("Incident not found or not assigned to you!");
                }

                incident.Status = IncidentStatus.Closed;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident {IncidentId} closed by Officer {OfficerId}", incidentId, officerId);

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex) when (ex.Message != "Incident not found or not assigned to you!")
            {
                _logger.LogError(ex, "Error closing incident {IncidentId}", incidentId);
                throw new Exception("Failed to close incident. Please try again later.");
            }
        }

        // VERIFY INCIDENT (Station Head)
        public async Task<IncidentResponseDTO> VerifyIncidentAsync(int incidentId)
        {
            try
            {
                var incident = await _context.Incidents
                    .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

                if (incident == null)
                {
                    _logger.LogWarning("Verify failed - Incident not found: {IncidentId}", incidentId);
                    throw new Exception("Incident not found!");
                }

                if (incident.Status != IncidentStatus.Closed)
                {
                    _logger.LogWarning("Verify failed - Incident {IncidentId} not closed yet", incidentId);
                    throw new Exception("Incident must be closed before verifying!");
                }

                incident.Status = IncidentStatus.Verified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident {IncidentId} verified", incidentId);

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex) when (ex.Message is not ("Incident not found!" or "Incident must be closed before verifying!"))
            {
                _logger.LogError(ex, "Error verifying incident {IncidentId}", incidentId);
                throw new Exception("Failed to verify incident. Please try again later.");
            }
        }

        // MAP TO DTO 
        private IncidentResponseDTO MapToDTO(Incident incident)
        {
            return _mapper.Map<IncidentResponseDTO>(incident);
        }
    }
}