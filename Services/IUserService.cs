using health_information.Models.Dto;

namespace health_information.Services
{
    public interface IUserService
    {
        Task<bool> CreateAsync(UserDto dto);
        Task<UserReadonlyDto> GetUserById(int id);
        Task<UserReadonlyDto> GetUserbyUserName(string name);
        Task<UserReadonlyDto> GetUserbyEmail(string email);
        Task<List<UserReadonlyDto>> GetUsersAsync();
        Task<bool> UpdateUser(UserDto dto);
        Task<bool> DeleteUser(int id);
        Task<LoginResponseDto> LoginUser(LoginDto login);
    }
}
