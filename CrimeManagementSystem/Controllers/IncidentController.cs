using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrimeManagementSystem.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class IncidentController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly IPdfService _pdfService;

        public IncidentController(IIncidentService incidentService, IPdfService pdfService)
        {
            _incidentService = incidentService;
            _pdfService = pdfService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

       
        [HttpPost("create")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _incidentService.CreateIncidentAsync(dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

      
        [HttpGet("my-incidents")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyIncidents()
        {
            try
            {
                var userId = GetUserId();
                var result = await _incidentService.GetUserIncidentsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncidentById(int id)
        {
            try
            {
                var result = await _incidentService.GetIncidentByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

       
        [HttpGet("all")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> GetAllIncidents()
        {
            try
            {
                var result = await _incidentService.GetAllIncidentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("assign")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> AssignOfficer([FromBody] AssignOfficerDTO dto)
        {
            try
            {
                var result = await _incidentService.AssignOfficerAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

 
        [HttpPut("close/{id}")]
        [Authorize(Roles = "Officer")]
        public async Task<IActionResult> CloseIncident(int id)
        {
            try
            {
                var officerId = GetUserId();
                var result = await _incidentService.CloseIncidentAsync(id, officerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpPut("verify/{id}")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> VerifyIncident(int id)
        {
            try
            {
                var result = await _incidentService.VerifyIncidentAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadIncidentReport(int id)
        {
            try
            {
                var incident = await _incidentService.GetIncidentByIdAsync(id);
                var pdfBytes = _pdfService.GenerateIncidentReport(incident);

                return File(pdfBytes, "application/pdf",
                    $"Incident_{incident.IncidentCode}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}