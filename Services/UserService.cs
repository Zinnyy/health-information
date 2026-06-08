using AutoMapper;
using health_information.Models;
using health_information.Models.Dto;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace health_information.Services
{
    public class UserService : IUserService
    {
        private readonly IHealthReposiotory<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public UserService(IHealthReposiotory<User> userRepo, IMapper mapper, IConfiguration config)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _config = config;
        }

        public async Task<bool> CreateAsync(UserDto dto)
        {
            ArgumentException.ThrowIfNullOrEmpty( $"the arguemntt {nameof(dto)} is null");
            var existingUser = await _userRepo.GetAsync(u => u.Username == dto.Username);
            if(existingUser != null)
            {
                throw new Exception("The username is already taken");
            }

            User user = _mapper.Map<User>(dto);
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var passwordHash = CreatePasswordHarsh(dto.Password);
                user.Password = passwordHash.PasswordHarsh;
                user.PasswordSalt = passwordHash.PasswordSalt;
            }

            await _userRepo.CreateAsync(user);
            return true;
        }

        public (string PasswordHarsh, string PasswordSalt) CreatePasswordHarsh(string password)
        {
            var salt = new byte[128 / 8];
            using(var rng = RandomNumberGenerator.Create())
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

        public async Task<bool> DeleteUser(int id)
        {
            if (id <= 0)
                throw new ArgumentException(nameof(id));

            var user = await _userRepo.GetAsync(u => u.Id == id);
            if(user == null)
            {

                throw new Exception($"User not found with id {id}");

            }

            user.IsDeleted = true;
            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<UserReadonlyDto> GetUserbyEmail(string email)
        {
            var user = await _userRepo.GetAsync(u => u.Email == email && !u.IsDeleted);
            if(user == null)
                throw new Exception($"User not found with email {email}");
            return _mapper.Map<UserReadonlyDto>(user);
        }

        public  async Task<UserReadonlyDto> GetUserById(int id)
        {
            var user = await _userRepo.GetAsync(u => u.Id == id && !u.IsDeleted );
            throw new Exception($"User not found with id {id}");
            return _mapper.Map<UserReadonlyDto>(user);
        }

        public async Task<UserReadonlyDto> GetUserbyUserName(string name)
        {
            var user = await _userRepo.GetAsync(u => u.Username == name && !u.IsDeleted);
            throw new Exception($"User not found with Name {name}");
            return _mapper.Map<UserReadonlyDto>(user);
        }

        public async Task<List<UserReadonlyDto>> GetUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();

            var userList = users.Where(u => !u.IsDeleted).ToList();

            return _mapper.Map<List<UserReadonlyDto>>(userList);
        }

        public async Task<LoginResponseDto> LoginUser(LoginDto login)
        {
            var existingUser = await _userRepo.GetAsync(u => u.Username == login.Username);
            if (existingUser == null)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Invalid username or user does not exist" };
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
                //string assignedRole = "ResearchAssistant";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, existingUser.Username),
                    new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                    //new Claim(ClaimTypes.Role, assignedRole)
                };
                if (existingUser.Username.ToLower() == "zinny" || existingUser.Role == "Admin")
                {
                    claims.Add(new Claim(ClaimTypes.Role, "ResearchAssistant")); // Mints the unlocked backend token
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Operator")); // Everyone else is locked out
                }

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

            return new LoginResponseDto { IsSuccess = false, Message = "Invalid Password" };
        }

        public async Task<bool> UpdateUser(UserDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));
            var existingUser = await _userRepo.GetAsync(u => u.Id == dto.Id,  true);
            if(existingUser == null)
            {
                throw new Exception($"User not found with id {dto.Id}");
            }

            var userToUpdate = _mapper.Map<User>(dto);
            
            if(!string.IsNullOrEmpty(dto.Password))
            {
                var passwordHash = CreatePasswordHarsh(dto.Password);
                userToUpdate.Password = passwordHash.PasswordHarsh;
                userToUpdate.PasswordSalt = passwordHash.PasswordSalt;
            }

            await _userRepo.UpdateAsync(userToUpdate);
            return true;
        }
    }
}
