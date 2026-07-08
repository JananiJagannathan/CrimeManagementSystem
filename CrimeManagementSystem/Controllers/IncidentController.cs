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

        // POST: api/v1/Incident/upload-graffiti
        // User uploads graffiti image when reporting
        [HttpPost("upload-graffiti")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UploadGraffitiImage(IFormFile file)
        {
            try
            {
                // Validate file exists
                if (file == null || file.Length == 0)
                    return BadRequest(new { Message = "No image file uploaded." });

                // Validate file type (images only)
                var allowedTypes = new[]
                {
            "image/jpeg", "image/jpg",
            "image/png", "image/gif", "image/webp"
        };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(new
                    {
                        Message = "Only image files are allowed " +
                                  "(jpg, jpeg, png, gif, webp)."
                    });

                // Validate file size (max 10MB for graffiti images)
                if (file.Length > 10 * 1024 * 1024)
                    return BadRequest(new
                    {
                        Message = "Graffiti image size cannot exceed 10MB."
                    });

                // Create uploads folder
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Uploads", "GraffitiImages");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename with timestamp
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"GRAFFITI_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return the relative path to use in CreateIncidentDTO
                var relativePath = $"/Uploads/GraffitiImages/{uniqueFileName}";

                return Ok(new
                {
                    Message = "Graffiti image uploaded successfully!",
                    ImagePath = relativePath,
                    FileName = uniqueFileName,
                    FileSize = $"{file.Length / 1024} KB"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "Failed to upload graffiti image. Please try again."
                });
            }
        }
    }
}