using health_information.Models;
using health_information.Models.Dto;

namespace health_information.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
        Task<AdminUserDto> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(AdminUserUpsertDto dto); 
        Task<bool> UpdateUserAsync( AdminUserUpsertDto dto);
        Task<bool> DeleteUserAsync(int id);
        Task<LoginResponseDto> LoginAdminAsync(LoginDto login);
        Task<List<AdminPatientDto>> GetAllPatientDetailAsync();
        Task<bool> PurgePatientRecordsAsync(int patientId);
        Task<bool> DeleteAsync(int id);
    }
}
