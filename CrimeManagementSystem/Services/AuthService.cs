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

        public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _emailService = emailService;
        }

        // REGISTER USER
        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning("Duplicate registration attempt: {Email}", dto.Email);
                    throw new Exception("Email already registered!");
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

                // Send welcome email
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Welcome to Crime Management System",
                    $"<h2>Welcome, {user.Name}!</h2><p>Your account has been successfully created.</p><p>You can now report incidents and track their status.</p>");

                return new AuthResponseDTO
                {
                    Token = GenerateToken(user.UserId.ToString(), user.Email, user.Role.ToString()),
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };
            }
            catch (Exception ex) when (ex.Message != "Email already registered!")
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", dto.Email);
                throw new Exception("Registration failed. Please try again later.");
            }
        }

        // LOGIN 
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
                        _logger.LogWarning("Failed login attempt (wrong password) for {Email}", dto.Email);
                        throw new Exception("Invalid email or password!");
                    }

                    // Send login notification email
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Login Notification - Crime Management System",
                        $"<p>Hello {user.Name},</p><p>You have successfully logged in at {DateTime.Now:dd-MM-yyyy HH:mm}.</p>");

                    return new AuthResponseDTO
                    {
                        Token = GenerateToken(user.UserId.ToString(), user.Email, user.Role.ToString()),
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
                        _logger.LogWarning("Failed login attempt (wrong password) for officer {Email}", dto.Email);
                        throw new Exception("Invalid email or password!");
                    }

                    return new AuthResponseDTO
                    {
                        Token = GenerateToken(officer.OfficerId.ToString(), officer.Email, "Officer"),
                        Name = officer.Name,
                        Email = officer.Email,
                        Role = "Officer"
                    };
                }

                _logger.LogWarning("Login attempt for non-existent email: {Email}", dto.Email);
                throw new Exception("Invalid email or password!");
            }
            catch (Exception ex) when (ex.Message != "Invalid email or password!")
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", dto.Email);
                throw new Exception("Login failed. Please try again later.");
            }
        }

        // REGISTER STATION HEAD 
        public async Task<AuthResponseDTO> RegisterStationHeadAsync(RegisterDTO dto)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingUser != null)
                    throw new Exception("Email already registered!");

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

                _logger.LogInformation("Station Head created with ID: {UserId}", stationHead.UserId);

                return new AuthResponseDTO
                {
                    Token = GenerateToken(stationHead.UserId.ToString(), stationHead.Email, stationHead.Role.ToString()),
                    Name = stationHead.Name,
                    Email = stationHead.Email,
                    Role = stationHead.Role.ToString()
                };
            }
            catch (Exception ex) when (ex.Message != "Email already registered!")
            {
                _logger.LogError(ex, "Unexpected error during Station Head registration for {Email}", dto.Email);
                throw new Exception("Registration failed. Please try again later.");
            }
        }

        // GET PROFILE 
        public async Task<ProfileResponseDTO> GetProfileAsync(int userId, string role)
        {
            try
            {
                if (role == "Officer")
                {
                    var officer = await _context.Officers.FirstOrDefaultAsync(o => o.OfficerId == userId);
                    if (officer == null)
                        throw new Exception("Profile not found!");

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
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null)
                        throw new Exception("Profile not found!");

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
            catch (Exception ex) when (ex.Message != "Profile not found!")
            {
                _logger.LogError(ex, "Error fetching profile for {UserId}", userId);
                throw new Exception("Failed to fetch profile. Please try again later.");
            }
        }

        // UPDATE PROFILE 
        public async Task<ProfileResponseDTO> UpdateProfileAsync(int userId, string role, UpdateProfileDTO dto)
        {
            try
            {
                if (role == "Officer")
                {
                    var officer = await _context.Officers.FirstOrDefaultAsync(o => o.OfficerId == userId);
                    if (officer == null)
                        throw new Exception("Profile not found!");

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
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null)
                        throw new Exception("Profile not found!");

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
            catch (Exception ex) when (ex.Message != "Profile not found!")
            {
                _logger.LogError(ex, "Error updating profile for {UserId}", userId);
                throw new Exception("Failed to update profile. Please try again later.");
            }
        }

        // CHANGE PASSWORD 
        public async Task<bool> ChangePasswordAsync(int userId, string role, ChangePasswordDTO dto)
        {
            try
            {
                if (role == "Officer")
                {
                    var officer = await _context.Officers.FirstOrDefaultAsync(o => o.OfficerId == userId);
                    if (officer == null)
                        throw new Exception("Profile not found!");

                    bool isValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, officer.PasswordHash);
                    if (!isValid)
                        throw new Exception("Current password is incorrect!");

                    officer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Officer password changed: {OfficerId}", userId);
                    return true;
                }
                else
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user == null)
                        throw new Exception("Profile not found!");

                    bool isValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
                    if (!isValid)
                        throw new Exception("Current password is incorrect!");

                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User password changed: {UserId}", userId);
                    return true;
                }
            }
            catch (Exception ex) when (ex.Message is not ("Profile not found!" or "Current password is incorrect!"))
            {
                _logger.LogError(ex, "Error changing password for {UserId}", userId);
                throw new Exception("Failed to change password. Please try again later.");
            }
        }

        // GENERATE JWT TOKEN 
        private string GenerateToken(string userId, string email, string role)
        {
            var jwt = _config.GetSection("JwtSettings");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwt["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}