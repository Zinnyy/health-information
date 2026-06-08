using AutoMapper;
using health_information.Models;
using health_information.Models.Dto;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace health_information.Services
{
    public class AdminService : IAdminService
    {
    private readonly IHealthReposiotory<Admin> _adminRepo;
        private readonly IHealthReposiotory<User> _userRepo;
        private readonly IHealthReposiotory<Patient> _patientRepo;
        private readonly IHealthReposiotory<VitalRecord> _vitalRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AdminService(IHealthReposiotory<Admin> adminRepo, IHealthReposiotory<Patient> patientRepo, IHealthReposiotory<User> userRepo, IHealthReposiotory<VitalRecord> vitalRepo, IMapper mapper, IConfiguration config)
        {
            _adminRepo = adminRepo;
            _mapper = mapper;
            _config = config;
            _patientRepo = patientRepo;
            _vitalRepo = vitalRepo;
            _userRepo = userRepo;
        }
        public async Task<bool> CreateUserAsync(AdminUserUpsertDto dto)
        {
            var exists = await _adminRepo.GetAsync(u => u.Username == dto.Username, useNoTracking: true);
            if (exists != null) return false;

            Admin user = _mapper.Map<Admin>(dto);
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var passwordHash = CreatePasswordHarsh(dto.Password);
                user.Password = passwordHash.PasswordHarsh;
                user.PasswordSalt = passwordHash.PasswordSalt;
            }

            await _adminRepo.CreateAsync(user);
            return true;
        }

        public (string PasswordHarsh, string PasswordSalt) CreatePasswordHarsh(string password)
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8

                ));

            return (PasswordHarsh: hash, PasswordSalt: Convert.ToBase64String(salt));
        }


        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _adminRepo.GetAsync(u => u.Id == id);
            if (user == null) return false;
            return await _adminRepo.DeleteAsync(user);
        }

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<AdminUserDto>>(users);
        }

        public async Task<AdminUserDto> GetUserByIdAsync(int id)
        {
            var users = await _userRepo.GetAsync(u => u.Id == id);
            if (users == null) return null;

            return _mapper.Map<AdminUserDto>(users);
        }

        public async Task<bool> UpdateUserAsync(AdminUserUpsertDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));
            var existingUser = await _adminRepo.GetAsync(u => u.Id == dto.Id, true);
            if (existingUser == null)
            {
                throw new Exception($"User not found with id {dto.Id}");
            }

            var userToUpdate = _mapper.Map<Admin>(dto);

            if (!string.IsNullOrEmpty(dto.Password))
            {
                var passwordHash = CreatePasswordHarsh(dto.Password);
                userToUpdate.Password = passwordHash.PasswordHarsh;
                userToUpdate.PasswordSalt = passwordHash.PasswordSalt;
            }

            await _adminRepo.UpdateAsync(userToUpdate);
            return true;
        }

        public async Task<LoginResponseDto> LoginAdminAsync(LoginDto login)
        {
            if(login.Username.ToLower() != "zinny")
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Access Denied: Invalid Administrator parameters or account inactive" };
            }
            var existingUser = await _adminRepo.GetAsync(a => a.Username.ToLower() == login.Username.ToLower(),
            useNoTracking: true);
            if (existingUser == null)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Invalid username or user does not exist" };
            }

            bool isAuthorizedAdmin = existingUser.Username.ToLower() == "zinny" || existingUser.Role == "Admin";
            if (!isAuthorizedAdmin)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Access Denied: Invalid Administrator parameters or account inactive" };
            }

            var passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: login.Password,
                salt: Convert.FromBase64String(existingUser.PasswordSalt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
                ));

            var isValid = CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(existingUser.Password),
                Convert.FromBase64String(passwordHash)
                );

            if (isValid)
            {
                var key = Encoding.ASCII.GetBytes(_config["UserSecret"] ?? throw new InvalidOperationException("UserSecret key is missing in configuration."));
                var tokenHandler = new JwtSecurityTokenHandler();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, existingUser.Username),
                    new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, existingUser.Role)
                };

                var tokenDescription = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(4),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256
                        )
                };

                var token = tokenHandler.CreateToken(tokenDescription);

                return new LoginResponseDto
                {
                    IsSuccess = true,
                    Message = "Login Successful",
                    Token = tokenHandler.WriteToken(token)
                };
            }

            return new LoginResponseDto { IsSuccess = false, Message = "Invalid password" };
        }

        public async Task<List<AdminPatientDto>> GetAllPatientDetailAsync()
        {
            var patients = await _patientRepo.GetAllByFilterAsync(p => p.Id > 0, IncludeProperties: "CreatedBy, VitalRecords");

            return _mapper.Map<List<AdminPatientDto>>(patients);
        }

        public async Task<bool> PurgePatientRecordsAsync(int patientId)
        {
            var patient = await _patientRepo.GetAsync(p => p.Id == patientId, true);
            if(patient == null) return false;

            var childVitals = await _vitalRepo.GetAllByFilterAsync(v => v.PatientId == patientId);
            foreach (var vital in childVitals)
            {
                await _vitalRepo.DeleteAsync(vital);
            }

           
            return await _patientRepo.DeleteAsync(patient);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Patient Id");

            var admin = await _adminRepo.GetAsync(p => p.Id == id);
            if (admin == null)
                throw new Exception($"Patient with id {id} does not exist");

            return await _adminRepo.DeleteAsync(admin);
        }
    }
}
