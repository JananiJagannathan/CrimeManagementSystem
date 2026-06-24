using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrimeManagementSystem.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class OfficerController : ControllerBase
    {
        private readonly IOfficerService _officerService;

        public OfficerController(IOfficerService officerService)
        {
            _officerService = officerService;
        }

    
        [HttpPost("add")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> AddOfficer([FromBody] AddOfficerDTO dto)
        {
            try
            {
                var result = await _officerService.AddOfficerAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

     
        [HttpGet("all")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> GetAllOfficers()
        {
            try
            {
                var result = await _officerService.GetAllOfficersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

  
        [HttpGet("{id}")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> GetOfficerById(int id)
        {
            try
            {
                var result = await _officerService.GetOfficerByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

     
        [HttpDelete("remove/{id}")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> RemoveOfficer(int id)
        {
            try
            {
                var result = await _officerService.RemoveOfficerAsync(id);
                return Ok(new { Message = "Officer removed successfully!", Success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

  
        [HttpGet("incidents/{id}")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> GetOfficerIncidents(int id)
        {
            try
            {
                var result = await _officerService.GetOfficerIncidentsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("my-incidents")]
        [Authorize(Roles = "Officer")]
        public async Task<IActionResult> GetMyIncidents()
        {
            try
            {
                var officerIdClaim = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var officerId = int.Parse(officerIdClaim!);
                var result = await _officerService.GetOfficerIncidentsAsync(officerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}