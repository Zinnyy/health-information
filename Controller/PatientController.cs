using health_information.Models.Dto;
using health_information.Services;
using Microsoft.AspNetCore.Mvc;

namespace health_information.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePatient(PatientCreateDto patientDto)
        {
            try
            {
                var result = await _patientService.CreateAsync(patientDto);
                if (result)
                {
                    return Ok(new { message = "Patient created successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to create patient" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient != null)
            {
                return Ok(patient);
            }
            else
            {
                return NotFound(new { message = $"Patient with id {id} not found" });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _patientService.GetAllAsync();
            return Ok(patients);
        }

        [HttpGet("vitals/{id}")]
        public async Task<IActionResult> GetPatientWithVitals(int id)
        {
            var patientWithVitals = await _patientService.GetpatientWithVitalsAsync(id);
            if (patientWithVitals != null)
            {
                return Ok(patientWithVitals);
            }
            else
            {
                return NotFound(new { message = $"Patient profile with id {id} not found" });
            }
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdatePatient(PatientDto patientDto)
        {
            try
            {
                var result = await _patientService.UpdateAsync(patientDto);
                if (result)
                {
                    return Ok(new { message = "Patient updated successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update patient" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("study/{studyId}")]
        public async Task<IActionResult> GetPatientByStudyId(string studyId)
        { 
             var patient = await _patientService.GetByStudyIdAsync(studyId);

            if (patient == null)
            {
                return NotFound(new { message = $"Patient with Study ID '{studyId}' not found." });
            }
           
            return Ok(patient);

        
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
                var result = await _patientService.DeleteAsync(id);
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
