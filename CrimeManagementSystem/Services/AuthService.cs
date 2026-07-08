using CrimeManagementSystem.Data;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Enums;
using CrimeManagementSystem.Interfaces;
using CrimeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CrimeManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration config,
            ILogger<AuthService> logger, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _emailService = emailService;
        }

        // ── REGISTER USER ─────────────────────────────────────
        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning("Duplicate registration attempt: {Email}", dto.Email);
                    throw new Exceptions.DuplicateResourceException(
                        $"An account with email '{dto.Email}' already exists. " +
                        $"Please use a different email or login to your existing account.");
                }

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    AadhaarNumber = dto.AadhaarNumber,
                    PANNumber = dto.PANNumber,
                    DateOfBirth = dto.DateOfBirth,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Role = UserRole.User,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user created with ID: {UserId}", user.UserId);

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Welcome to Crime Management System",
                    $"<h2>Welcome, {user.Name}!</h2>" +
                    $"<p>Your account has been successfully created.</p>" +
                    $"<p>You can now report incidents and track their status.</p>");

                return new AuthResponseDTO
                {
                    Token = GenerateToken(user.UserId.ToString(),
                        user.Email, user.Role.ToString()),
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };
            }
            catch (Exception ex) when (ex is not Exceptions.DuplicateResourceException)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", dto.Email);
                throw new Exception("Registration failed. Please try again later.");
            }
        }

        // ── LOGIN ─────────────────────────────────────────────
        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user != null)
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

                    if (!isValid)
                    {
                        _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
                        throw new Exceptions.PasswordException(
                            "The password entered is incorrect. Please try again.");
                    }

                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Login Notification - Crime Management System",
                        $"<p>Hello {user.Name},</p>" +
                        $"<p>You have successfully logged in at " +
                        $"{DateTime.Now:dd-MM-yyyy HH:mm}.</p>" +
                        $"<p>If this wasn't you, please change your password immediately.</p>");

                    return new AuthResponseDTO
                    {
                        Token = GenerateToken(user.UserId.ToString(),
                            user.Email, user.Role.ToString()),
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString()
                    };
                }

                var officer = await _context.Officers
                    .FirstOrDefaultAsync(o => o.Email == dto.Email);

                if (officer != null)
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, officer.PasswordHash);

                    if (!isValid)
                    {
                        _logger.LogWarning("Failed login for officer {Email}", dto.Email);
                        throw new Exceptions.PasswordException(
                            "The password entered is incorrect. Please try again.");
                    }

                    // ✅ Officer login email notification
                    await _emailService.SendEmailAsync(
                        officer.Email,
                        "Login Notification - Crime Management System",
                        $"<p>Hello Officer {officer.Name},</p>" +
                        $"<p>You have successfully logged in at " +
                        $"{DateTime.Now:dd-MM-yyyy HH:mm}.</p>" +
                        $"<p>If this wasn't you, please contact the " +
                        $"Station Head immediately.</p>");

                    return new AuthResponseDTO
                    {
                        Token = GenerateToken(officer.OfficerId.ToString(),
                            officer.Email, "Officer"),
                        Name = officer.Name,
                        Email = officer.Email,
                        Role = "Officer"
                    };
                }

                _logger.LogWarning("Login attempt for non-existent email: {Email}", dto.Email);
                throw new Exceptions.TokenException(
                    "No account found with this email address. Please register first.");
            }
            catch (Exception ex) when (ex is not Exceptions.PasswordException
                                    && ex is not Exceptions.TokenException)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", dto.Email);
                throw new Exception("Login failed. Please try again later.");
            }
        }

        // ── REGISTER STATION HEAD ─────────────────────────────
        public async Task<AuthResponseDTO> RegisterStationHeadAsync(RegisterDTO dto)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingUser != null)
                {
                    throw new Exceptions.DuplicateResourceException(
                        $"An account with email '{dto.Email}' already exists in the system.");
                }

                var stationHead = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    AadhaarNumber = dto.AadhaarNumber,
                    PANNumber = dto.PANNumber,
                    DateOfBirth = dto.DateOfBirth,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Role = UserRole.StationHead,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(stationHead);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Station Head created with ID: {UserId}",
                    stationHead.UserId);

                return new AuthResponseDTO
                {
                    Token = GenerateToken(stationHead.UserId.ToString(),
                        stationHead.Email, stationHead.Role.ToString()),
                    Name = stationHead.Name,
                    Email = stationHead.Email,
                    Role = stationHead.Role.ToString()
                };
            }
            catch (Exception ex) when (ex is not Exceptions.DuplicateResourceException)
            {
                _logger.LogError(ex, "Error during Station Head registration for {Email}",
                    dto.Email);
                throw new Exception("Registration failed. Please try again later.");
            }
        }

        // ── GET PROFILE ────────────────────────────────────────
        public async Task<ProfileResponseDTO> GetProfileAsync(int userId, string role)
        {
            try
            {
                if (role == "Officer")
                {
                    var officer = await _context.Officers
                        .FirstOrDefaultAsync(o => o.OfficerId == userId);
                    if (officer == null)
                        throw new Exceptions.NotFoundException("Profile not found!");

                    return new ProfileResponseDTO
                    {
                        Id = officer.OfficerId,
                        Name = officer.Name,
                        Email = officer.Email,
                        Phone = officer.Phone,
                        Role = "Officer",
                        ProfilePicture = null
                    };
                }
                else
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null)
                        throw new Exceptions.NotFoundException("Profile not found!");

                    return new ProfileResponseDTO
                    {
                        Id = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = user.Phone,
                        Role = user.Role.ToString(),
                        ProfilePicture = user.ProfilePicture
                    };
                }
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error fetching profile for {UserId}", userId);
                throw new Exception("Failed to fetch profile. Please try again later.");
            }
        }

        // ── UPDATE PROFILE ─────────────────────────────────────
        public async Task<ProfileResponseDTO> UpdateProfileAsync(int userId,
            string role, UpdateProfileDTO dto)
        {
            try
            {
                if (role == "Officer")
                {
                    var officer = await _context.Officers
                        .FirstOrDefaultAsync(o => o.OfficerId == userId);
                    if (officer == null)
                        throw new Exceptions.NotFoundException("Profile not found!");

                    officer.Name = dto.Name;
                    officer.Phone = dto.Phone;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Officer profile updated: {OfficerId}", userId);

                    return new ProfileResponseDTO
                    {
                        Id = officer.OfficerId,
                        Name = officer.Name,
                        Email = officer.Email,
                        Phone = officer.Phone,
                        Role = "Officer"
                    };
                }
                else
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null)
                        throw new Exceptions.NotFoundException("Profile not found!");

                    user.Name = dto.Name;
                    user.Phone = dto.Phone;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User profile updated: {UserId}", userId);

                    return new ProfileResponseDTO
                    {
                        Id = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = user.Phone,
                        Role = user.Role.ToString(),
                        ProfilePicture = user.ProfilePicture
                    };
                }
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException)
            {
                _logger.LogError(ex, "Error updating profile for {UserId}", userId);
                throw new Exception("Failed to update profile. Please try again later.");
            }
        }

        // ── CHANGE PASSWORD ─────────────────────────────────────
        public async Task<bool> ChangePasswordAsync(int userId,
            string role, ChangePasswordDTO dto)
        {
            try
            {
                if (role == "Officer")
                {
                    var officer = await _context.Officers
                        .FirstOrDefaultAsync(o => o.OfficerId == userId);
                    if (officer == null)
                        throw new Exceptions.NotFoundException("Profile not found!");

                    bool isValid = BCrypt.Net.BCrypt
                        .Verify(dto.CurrentPassword, officer.PasswordHash);
                    if (!isValid)
                        throw new Exceptions.PasswordException(
                            "The current password you entered is incorrect. Please try again.");

                    officer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Officer password changed: {OfficerId}", userId);
                    return true;
                }
                else
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null)
                        throw new Exceptions.NotFoundException("Profile not found!");

                    bool isValid = BCrypt.Net.BCrypt
                        .Verify(dto.CurrentPassword, user.PasswordHash);
                    if (!isValid)
                        throw new Exceptions.PasswordException(
                            "The current password you entered is incorrect. Please try again.");

                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User password changed: {UserId}", userId);
                    return true;
                }
            }
            catch (Exception ex) when (ex is not Exceptions.NotFoundException
                                    && ex is not Exceptions.PasswordException)
            {
                _logger.LogError(ex, "Error changing password for {UserId}", userId);
                throw new Exception("Failed to change password. Please try again later.");
            }
        }

        // ── GENERATE JWT TOKEN ────────────────────────────────
        private string GenerateToken(string userId, string email, string role)
        {
            var jwt = _config.GetSection("JwtSettings");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    Convert.ToDouble(jwt["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ── FORGOT PASSWORD ───────────────────────────────────
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDTO dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Forgot password for non-existent: {Email}", dto.Email);
                    throw new Exceptions.NotFoundException(
                        "No account found with this email address. Please register first.");
                }
                // Generate 6-digit OTP
                var random = new Random();
                var otp = random.Next(100000, 999999).ToString();

                // Store OTP with 15 minute expiry
                user.PasswordResetToken = otp;
                user.PasswordResetTokenExpiry = DateTime.Now.AddMinutes(15);
                await _context.SaveChangesAsync();

                _logger.LogInformation("OTP generated for: {Email}", dto.Email);

                // Send OTP email
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset OTP - Crime Management System",
                    $"<div style='font-family:Arial;max-width:500px;margin:auto'>" +
                    $"<h2 style='color:#1a237e'>Password Reset OTP</h2>" +
                    $"<p>Dear {user.Name},</p>" +
                    $"<p>Your OTP to reset your password is:</p>" +
                    $"<div style='background:#e8eaf6;border-radius:10px;" +
                    $"padding:20px;text-align:center;margin:20px 0'>" +
                    $"<h1 style='color:#1a237e;font-size:48px;" +
                    $"letter-spacing:15px;margin:0'>{otp}</h1>" +
                    $"</div>" +
                    $"<p style='color:#666'>This OTP expires in " +
                    $"<b>15 minutes</b>.</p>" +
                    $"<p style='color:#666'>If you didn't request this, " +
                    $"please ignore this email.</p>" +
                    $"<br><p><b>Crime Management System</b></p>" +
                    $"</div>");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password for {Email}", dto.Email);
                throw new Exceptions.NotFoundException(
                         "No account found with this email address. Please register first.");
            }
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpDTO dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email
                                       && u.PasswordResetToken == dto.Otp);

                if (user == null)
                {
                    _logger.LogWarning("Invalid OTP attempt for: {Email}", dto.Email);
                    throw new Exceptions.TokenException(
                        "Invalid OTP. Please check your email and try again.");
                }

                if (user.PasswordResetTokenExpiry < DateTime.Now)
                {
                    // Clear expired OTP
                    user.PasswordResetToken = null;
                    user.PasswordResetTokenExpiry = null;
                    await _context.SaveChangesAsync();

                    throw new Exceptions.TokenException(
                        "OTP has expired. Please request a new one.");
                }

                _logger.LogInformation("OTP verified for: {Email}", dto.Email);
                return true;
            }
            catch (Exception ex) when (ex is not Exceptions.TokenException)
            {
                _logger.LogError(ex, "Error verifying OTP for {Email}", dto.Email);
                throw new Exception("Failed to verify OTP. Please try again.");
            }
        }

        // ── RESET PASSWORD ────────────────────────────────────
        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email
                                       && u.PasswordResetToken == dto.Token);

                if (user == null)
                {
                    _logger.LogWarning("Invalid reset token attempt for email: {Email}", dto.Email);
                    throw new Exceptions.TokenException(
                        "Invalid or expired password reset link. " +
                        "Please request a new password reset.");
                }

                // Check token expiry
                if (user.PasswordResetTokenExpiry < DateTime.Now)
                {
                    _logger.LogWarning("Expired reset token for email: {Email}", dto.Email);

                    // Clear expired token
                    user.PasswordResetToken = null;
                    user.PasswordResetTokenExpiry = null;
                    await _context.SaveChangesAsync();

                    throw new Exceptions.TokenException(
                        "Your password reset link has expired. " +
                        "Please request a new password reset.");
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                // Invalidate the token (one-time use only!)
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password successfully reset for: {Email}", dto.Email);

                // Send confirmation email
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset Successful - Crime Management System",
                    $"<h2>Password Reset Successful ✅</h2>" +
                    $"<p>Dear {user.Name},</p>" +
                    $"<p>Your password has been successfully reset.</p>" +
                    $"<p>You can now login with your new password.</p>" +
                    $"<p>If you did not make this change, please contact " +
                    $"support immediately.</p>" +
                    $"<br><p><b>Crime Management System</b></p>");

                return true;
            }
            catch (Exception ex) when (ex is not Exceptions.TokenException)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", dto.Email);
                throw new Exception("Failed to reset password. Please try again.");
            }
        }
    }
}