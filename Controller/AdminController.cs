using health_information.Models.Dto;
using health_information.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace health_information.Controller
{
    [ApiController]
    [Route("api/[controller]")]

    //[Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _adminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { isSuccess = false, message = $"User with id {id} not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser(AdminUserUpsertDto dto)
        {
            try
            {
                var result = await _adminService.CreateUserAsync(dto);
                if (result)
                {
                    return Ok(new { isSuccess = true, message = "User created successfully" });
                }
                else
                {
                    return BadRequest(new { isSuccess = false, message = "Failed to create user" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPut("users/update")]
        public async Task<IActionResult> UpdateUser(AdminUserUpsertDto dto)
        {
            try
            {
                var result = await _adminService.UpdateUserAsync(dto);
                if (result)
                {
                    return Ok(new { isSuccess = true, message = "User updated successfully" });
                }
                else
                {
                    return BadRequest(new { isSuccess = false, message = "Failed to update user" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("users/delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(id);
                if (result)
                {
                    return Ok(new { isSuccess = true, message = "User deleted successfully" });
                }
                else
                {
                    return NotFound(new { isSuccess = false, message = $"User with id {id} not found" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAdmin(LoginDto login)
        {
            try
            {
                var result = await _adminService.LoginAdminAsync(login);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return Unauthorized(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpGet("patients/detailed")]
        public async Task<IActionResult> GetAllPatientDetails()
        {
            try
            {
                var patients = await _adminService.GetAllPatientDetailAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("patients/purge/{patientId}")]
        public async Task<IActionResult> PurgePatientRecords(int patientId)
        {
            try
            {
                var result = await _adminService.PurgePatientRecordsAsync(patientId);
                if (result)
                {
                    return Ok(new { isSuccess = true, message = "Patient records purged successfully" });
                }
                else
                {
                    return NotFound(new { isSuccess = false, message = $"Patient with id {patientId} not found" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
                var result = await _adminService.DeleteAsync(id);
                if (result)
                {
                    return Ok(new { message = "Patient deleted successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to delete patient" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
