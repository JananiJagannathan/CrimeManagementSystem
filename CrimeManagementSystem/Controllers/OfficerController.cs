using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Enums;
using CrimeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // POST: api/v1/officer/add
        // Only Station Head can add officer
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

        // GET: api/v1/officer/all
        // Only Station Head can view all officers
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

        // GET: api/v1/officer/{id}
        // Station Head can view individual officer
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

        // DELETE: api/v1/officer/remove/{id}
        // Only Station Head can remove officer
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

        // PUT: api/v1/officer/reactivate/{id}
        // Only Station Head can reactivate officer
        [HttpPut("reactivate/{id}")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> ReactivateOfficer(int id)
        {
            try
            {
                var result = await _officerService.ReactivateOfficerAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // GET: api/v1/officer/incidents/{id}
        // Station Head views officer's incidents
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

        // GET: api/v1/officer/my-incidents
        // Officer views their own assigned incidents
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

        // GET: api/v1/officer/all-users
        // Only Station Head can view all registered users
        [HttpGet("all-users")]
        [Authorize(Roles = "StationHead")]
        public async Task<IActionResult> GetAllUsers([FromServices] AppDbContext context)
        {
            try
            {
                var users = await context.Users
                    .Where(u => u.Role == UserRole.User)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Name,
                        u.Email,
                        u.Phone,
                        u.Address,
                        Role = u.Role.ToString(),
                        u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}