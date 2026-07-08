using CrimeManagementSystem.DTOs;

namespace CrimeManagementSystem.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
        Task<AuthResponseDTO> RegisterStationHeadAsync(RegisterDTO dto);
        Task<ProfileResponseDTO> GetProfileAsync(int userId, string role);
        Task<ProfileResponseDTO> UpdateProfileAsync(int userId, string role, UpdateProfileDTO dto);
        Task<bool> ChangePasswordAsync(int userId, string role, ChangePasswordDTO dto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDTO dto);
        Task<bool> VerifyOtpAsync(VerifyOtpDTO dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);         
    }
}