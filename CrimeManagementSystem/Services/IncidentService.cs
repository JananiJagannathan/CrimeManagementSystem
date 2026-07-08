using AutoMapper;
using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Enums;
using CrimeManagementSystem.Interfaces;
using CrimeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CrimeManagementSystem.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<IncidentService> _logger;
        private readonly IEmailService _emailService;

        public IncidentService(AppDbContext context, IMapper mapper,
            ILogger<IncidentService> logger, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _emailService = emailService;
        }

        // ── CREATE INCIDENT ───────────────────────────────────
        public async Task<IncidentResponseDTO> CreateIncidentAsync(
            CreateIncidentDTO dto, int userId)
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

                _logger.LogInformation("Incident created: {Code} by UserId {UserId}",
                    incidentCode, userId);

                // Send incident creation email to reporting user
                var reportingUser = await _context.Users.FindAsync(userId);
                if (reportingUser != null)
                {
                    await _emailService.SendEmailAsync(
                        reportingUser.Email,
                        $"Incident Registered Successfully - {incidentCode}",
                        $"<h2>Your Incident Has Been Registered</h2>" +
                        $"<p>Dear {reportingUser.Name},</p>" +
                        $"<p>Your incident has been successfully registered with the following details:</p>" +
                        $"<table style='border-collapse:collapse;width:100%'>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Incident ID:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{incidentCode}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Type:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{dto.Type}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Location:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{dto.Location}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Status:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>Initiated</td></tr>" +
                        $"</table>" +
                        $"<p>You can track your incident status using the Incident ID: <b>{incidentCode}</b></p>" +
                        $"<p>Our team will assign an officer to investigate your case shortly.</p>");
                }

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incident for UserId {UserId}", userId);
                throw new Exception("Failed to create incident. Please try again later.");
            }
        }

        // ── GET USER INCIDENTS ────────────────────────────────
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

        // ── GET INCIDENT BY ID ────────────────────────────────
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
                    throw new Exceptions.NotFoundException(
                        $"Incident with ID {incidentId} was not found in the system.");
                }

                return MapToDTO(incident);
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error fetching incident {IncidentId}", incidentId);
                throw new Exception("Failed to fetch incident. Please try again later.");
            }
        }

        // ── GET ALL INCIDENTS (Station Head) ──────────────────
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

        // ── ASSIGN OFFICER ────────────────────────────────────
        public async Task<IncidentResponseDTO> AssignOfficerAsync(AssignOfficerDTO dto)
        {
            try
            {
                var incident = await _context.Incidents
                    .Include(i => i.ReportedBy)
                    .FirstOrDefaultAsync(i => i.IncidentId == dto.IncidentId);

                if (incident == null)
                {
                    _logger.LogWarning("Assign failed - Incident not found: {IncidentId}",
                        dto.IncidentId);
                    throw new Exceptions.NotFoundException(
                        $"Incident with ID {dto.IncidentId} was not found in the system.");
                }

                var officer = await _context.Officers
                    .FirstOrDefaultAsync(o => o.OfficerId == dto.OfficerId);

                if (officer == null)
                {
                    _logger.LogWarning("Assign failed - Officer not found: {OfficerId}",
                        dto.OfficerId);
                    throw new Exceptions.NotFoundException(
                        $"Officer with ID {dto.OfficerId} was not found in the system.");
                }

                if (!officer.IsActive)
                {
                    _logger.LogWarning("Assign failed - Officer {OfficerId} is inactive",
                        dto.OfficerId);
                    throw new Exceptions.IncidentAssignmentException(
                        $"Officer with ID {dto.OfficerId} is currently inactive and cannot " +
                        $"be assigned to incidents. Please reactivate the officer first.");
                }

                incident.AssignedOfficerId = dto.OfficerId;
                incident.Status = IncidentStatus.Active;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident {IncidentId} assigned to Officer {OfficerId}",
                    dto.IncidentId, dto.OfficerId);

                // Send email to reporting user about officer assignment
                if (incident.ReportedBy != null)
                {
                    await _emailService.SendEmailAsync(
                        incident.ReportedBy.Email,
                        $"Officer Assigned to Your Incident - {incident.IncidentCode}",
                        $"<h2>An Officer Has Been Assigned to Your Incident</h2>" +
                        $"<p>Dear {incident.ReportedBy.Name},</p>" +
                        $"<p>Good news! An officer has been assigned to investigate your incident.</p>" +
                        $"<table style='border-collapse:collapse;width:100%'>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Incident ID:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{incident.IncidentCode}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Assigned Officer:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{officer.Name}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Badge Number:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{officer.BadgeNumber}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Current Status:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>Active - Under Investigation</td></tr>" +
                        $"</table>" +
                        $"<p>Your case is now actively being investigated. " +
                        $"You will be notified when there are further updates.</p>");
                }

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException
                                    && ex is not Exceptions.IncidentAssignmentException)
            {
                _logger.LogError(ex, "Error assigning officer to incident {IncidentId}",
                    dto.IncidentId);
                throw new Exception("Failed to assign officer. Please try again later.");
            }
        }

        // ── CLOSE INCIDENT (Officer) ──────────────────────────
        public async Task<IncidentResponseDTO> CloseIncidentAsync(int incidentId, int officerId)
        {
            try
            {
                var incident = await _context.Incidents
                    .Include(i => i.ReportedBy)
                    .FirstOrDefaultAsync(i => i.IncidentId == incidentId
                                        && i.AssignedOfficerId == officerId);

                if (incident == null)
                {
                    _logger.LogWarning(
                        "Close failed - Incident {IncidentId} not assigned to Officer {OfficerId}",
                        incidentId, officerId);
                    throw new Exceptions.IncidentAssignmentException(
                        $"Incident {incidentId} is not assigned to you. " +
                        $"Only the assigned officer can close this incident.");
                }

                incident.Status = IncidentStatus.Closed;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident {IncidentId} closed by Officer {OfficerId}",
                    incidentId, officerId);

                // Send email to reporting user about incident closure
                if (incident.ReportedBy != null)
                {
                    await _emailService.SendEmailAsync(
                        incident.ReportedBy.Email,
                        $"Your Incident Has Been Closed - {incident.IncidentCode}",
                        $"<h2>Incident Investigation Completed</h2>" +
                        $"<p>Dear {incident.ReportedBy.Name},</p>" +
                        $"<p>The investigation for your reported incident has been completed.</p>" +
                        $"<table style='border-collapse:collapse;width:100%'>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Incident ID:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{incident.IncidentCode}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Current Status:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>Closed - Pending Verification</td></tr>" +
                        $"</table>" +
                        $"<p>Your incident is now pending final verification by the Station Head. " +
                        $"You will receive another notification once the case is fully verified.</p>");
                }

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex) when (ex is not Exceptions.IncidentAssignmentException)
            {
                _logger.LogError(ex, "Error closing incident {IncidentId}", incidentId);
                throw new Exception("Failed to close incident. Please try again later.");
            }
        }

        // ── VERIFY INCIDENT (Station Head) ───────────────────
        public async Task<IncidentResponseDTO> VerifyIncidentAsync(int incidentId)
        {
            try
            {
                var incident = await _context.Incidents
                    .Include(i => i.ReportedBy)
                    .FirstOrDefaultAsync(i => i.IncidentId == incidentId);

                if (incident == null)
                {
                    _logger.LogWarning("Verify failed - Incident not found: {IncidentId}",
                        incidentId);
                    throw new Exceptions.NotFoundException(
                        $"Incident with ID {incidentId} was not found in the system.");
                }

                if (incident.Status != IncidentStatus.Closed)
                {
                    _logger.LogWarning("Verify failed - Incident {IncidentId} not closed yet",
                        incidentId);
                    throw new Exceptions.IncidentAssignmentException(
                        $"Incident {incidentId} cannot be verified because its current " +
                        $"status is '{incident.Status}'. " +
                        $"It must be 'Closed' by the assigned officer before " +
                        $"Station Head can verify it.");
                }

                incident.Status = IncidentStatus.Verified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Incident {IncidentId} verified", incidentId);

                // Send final verification email to reporting user
                if (incident.ReportedBy != null)
                {
                    await _emailService.SendEmailAsync(
                        incident.ReportedBy.Email,
                        $"Your Incident Has Been Verified - {incident.IncidentCode}",
                        $"<h2>Incident Successfully Verified ✅</h2>" +
                        $"<p>Dear {incident.ReportedBy.Name},</p>" +
                        $"<p>Great news! Your reported incident has been fully resolved " +
                        $"and verified by the Station Head.</p>" +
                        $"<table style='border-collapse:collapse;width:100%'>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Incident ID:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>{incident.IncidentCode}</td></tr>" +
                        $"<tr><td style='padding:8px;border:1px solid #ddd'><b>Final Status:</b></td>" +
                        $"<td style='padding:8px;border:1px solid #ddd'>Verified ✅</td></tr>" +
                        $"</table>" +
                        $"<p>This incident has been successfully closed and verified. " +
                        $"Thank you for reporting. Your cooperation helps keep our community safe.</p>" +
                        $"<p><b>Crime Management System</b></p>");
                }

                return await GetIncidentByIdAsync(incident.IncidentId);
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException
                                    && ex is not Exceptions.IncidentAssignmentException)
            {
                _logger.LogError(ex, "Error verifying incident {IncidentId}", incidentId);
                throw new Exception("Failed to verify incident. Please try again later.");
            }
        }

        // ── MAP TO DTO ────────────────────────────────────────
        private IncidentResponseDTO MapToDTO(Incident incident)
        {
            return _mapper.Map<IncidentResponseDTO>(incident);
        }
    }
}