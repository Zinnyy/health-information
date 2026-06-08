using health_information.Models.Dto;
using health_information.Services;
using Microsoft.AspNetCore.Mvc;

namespace health_information.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            try
            {
                var result = await _userService.CreateAsync(userDto);
                if (result)
                {
                    return Ok(new { message = "User registered successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to register user" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _userService.LoginUser(loginDto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound(new { message = "User not found" });
            }
        }
        [HttpGet("Getall")]
        public async Task<IActionResult> GetAllUsers()
        {
            var user = await _userService.GetUsersAsync();
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound(new { message = "Users not found" });
            }
        }
        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserbyUserName(username);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound(new { message = "User not found" });
            }
        }

        [HttpGet("Email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserbyEmail(email);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound(new { message = "User not found" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UserDto userDto)
        {
            try
            {
                if (userDto == null || userDto.Id <= 0)
                    return BadRequest(new { message = "Invalid user data" });

                var result = await _userService.UpdateUser(userDto);
                if (result)
                {
                    return Ok(new { message = "User updated successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update user" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUser(id);
                if (result)
                {
                    return Ok(new { message = "User deleted successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to delete user" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
    }
}
