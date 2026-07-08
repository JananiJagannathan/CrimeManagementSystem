using Asp.Versioning;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly Data.AppDbContext _context;

        public AuthController(IAuthService authService,
            ILogger<AuthController> logger,
            Data.AppDbContext context)
        {
            _authService = authService;
            _logger = logger;
            _context = context;
        }
        // ── Helper Methods ────────────────────────────────────
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        // POST: api/v1/Auth/register
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
                _logger.LogWarning("Register failed for {Email}: {Message}",
                    dto.Email, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/v1/Auth/login
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
                _logger.LogWarning("Login failed for {Email}: {Message}",
                    dto.Email, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/v1/Auth/register-stationhead
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

        // GET: api/v1/Auth/profile
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

        // PUT: api/v1/Auth/profile
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

        // PUT: api/v1/Auth/change-password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var role = GetUserRole();
                var result = await _authService.ChangePasswordAsync(userId, role, dto);
                return Ok(new
                {
                    Message = "Password changed successfully!",
                    Success = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/v1/Auth/logout
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("User logged out: {Email}", email);

            return Ok(new
            {
                Message = "Logged out successfully. " +
                          "Please remove your token from client storage."
            });
        }

        // POST: api/v1/Auth/forgot-password
        // No authorization needed — user is not logged in!
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            try
            {
                await _authService.ForgotPasswordAsync(dto);
                return Ok(new
                {
                    Message = "If an account exists with this email, " +
                              "a password reset link has been sent. " +
                              "Please check your inbox."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/v1/Auth/verify-otp
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDTO dto)
        {
            try
            {
                await _authService.VerifyOtpAsync(dto);
                return Ok(new { Message = "OTP verified successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/v1/Auth/reset-password
        // No authorization needed — user is not logged in!
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto);
                return Ok(new
                {
                    Message = "Password reset successfully! " +
                              "You can now login with your new password."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/v1/Auth/upload-profile-picture
        [HttpPost("upload-profile-picture")]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { Message = "No file uploaded." });

                // Validate file type
                var allowedTypes = new[]
                {
            "image/jpeg", "image/jpg",
            "image/png", "image/gif"
        };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(new
                    {
                        Message = "Only image files allowed (jpg, png, gif)."
                    });

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new
                    {
                        Message = "File size cannot exceed 5MB."
                    });

                // Create folder if not exists
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Uploads", "ProfilePictures");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"PROFILE_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/Uploads/ProfilePictures/{uniqueFileName}";

                // Update ProfilePicture in database
                var userId = GetUserId();
                var role = GetUserRole();

                if (role == "Officer")
                {
                    // Officers don't have ProfilePicture field
                    // Just return the path for frontend to store
                    _logger.LogInformation(
                        "Officer {Id} uploaded profile picture", userId);
                }
                else
                {
                    // Update User's ProfilePicture in DB
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                        return BadRequest(new { Message = "User not found." });

                    user.ProfilePicture = relativePath;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "User {Id} uploaded profile picture", userId);
                }

                return Ok(new
                {
                    Message = "Profile picture uploaded successfully!",
                    ProfilePicturePath = relativePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                return BadRequest(new
                {
                    Message = "Failed to upload profile picture. Please try again."
                });
            }
        }
    }
}