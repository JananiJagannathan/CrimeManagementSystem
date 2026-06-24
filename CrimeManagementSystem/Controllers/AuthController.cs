using CrimeManagementSystem.DTOs;
using Asp.Versioning;
using CrimeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrimeManagementSystem.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                _logger.LogInformation("Register attempt for email: {Email}", dto.Email);
                var result = await _authService.RegisterAsync(dto);
                _logger.LogInformation("User registered successfully: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Register failed for {Email}: {Message}", dto.Email, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", dto.Email);
                var result = await _authService.LoginAsync(dto);
                _logger.LogInformation("Login successful for: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Login failed for {Email}: {Message}", dto.Email, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("register-stationhead")]
        public async Task<IActionResult> RegisterStationHead([FromBody] RegisterDTO dto)
        {
            try
            {
                var result = await _authService.RegisterStationHeadAsync(dto);
                _logger.LogInformation("Station Head registered: {Email}", dto.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Station Head register failed: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
        }

        
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var role = GetUserRole();
                var result = await _authService.GetProfileAsync(userId, role);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var role = GetUserRole();
                var result = await _authService.UpdateProfileAsync(userId, role, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

      
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var role = GetUserRole();
                var result = await _authService.ChangePasswordAsync(userId, role, dto);
                return Ok(new { Message = "Password changed successfully!", Success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}