using health_information.Models.Dto;
using health_information.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace health_information.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class VitalRecordController : ControllerBase
    {
        private readonly IVitalRecordService _vitalService;
        private readonly IPatientService _patientService;

        public VitalRecordController(IVitalRecordService vitalService, IPatientService patientService)
        {
            _vitalService = vitalService;
            _patientService = patientService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVitalRecord(VitalRecordCreateDto vitalDto)
        {
            try
            {
                var result = await _vitalService.AddRecordAsync(vitalDto);
                if (result)
                {
                    return Ok(new { message = "Vital record created successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to create vital record" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("patient/{id}")]
        public async Task<IActionResult> GetVitalsByPatientId(int id)
        {
            try
            {
                var records = await _vitalService.GetRecordsByPatientIdAsync(id);
                if (records != null && records.Count > 0)
                {
                    return Ok(records);
                }
                else
                {
                    return NotFound(new { message = $"No vital records found for patient with id {id}" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVitalRecord(int id)
        {
            try
            {
                var result = await _vitalService.DeleteRecordAsync(id);
                if (result)
                {
                    return Ok(new { message = "Vital record deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = $"Vital record with id {id} not found" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("report/{patientid}")]
        public async Task<IActionResult> GeneratePaientChartsReport( int patientid)
        {
            //fetching the vital record
            var records = await _vitalService.GetRecordsByPatientIdAsync(patientid);
            if(records == null || !records.Any())
            {
                return NotFound(new { message = "No Clinical tracking data found for this patient to generate a chart" });
            }

            //fetch patient profile to pass their unique studyid to the python script
            var patientProfile = await _patientService.GetByIdAsync(patientid);
            string studyId = patientProfile?.PatientStudyId ?? $"PATIENT-{patientid}";


            //we convert the list of vitals into a json text string
            string jsonPayload = JsonSerializer.Serialize(records) ;


            //we configure the operating sys background process to trigger python
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"Scripts/generate_charts.py \"{studyId}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            try
            {
                using(Process process = Process.Start(startInfo))
                {
                    if (process == null)
                        return StatusCode(500, "Failed to initialize processing engine");


                    //open the way to push the json string directly into python memory
                    using(StreamWriter writer = process.StandardInput)
                    {
                        if (writer.BaseStream.CanWrite)
                        {
                            await writer.WriteAsync(jsonPayload);
                        }
                    }

                    string consoleOutput = await process.StandardOutput.ReadToEndAsync();
                    string terminalErrors = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();


                    //checking if python ran into an execution bug
                    if (!string.IsNullOrEmpty(terminalErrors))
                    {
                        return BadRequest(new { message = "Data pipeline breakdown inside script parsing.", details = terminalErrors });

                    }

                    //If successful, hand back a web link pointing to the generated image inside wwwroot
                    string browserFriendlyChartUrl = $"/charts/chart_{studyId}.png";
                    return Ok(new { isSuccess = true, message = "Chart generated Cleanly", chartUrl = browserFriendlyChartUrl });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500,new { message = "Process infrastructure initialization error.", error = ex.Message });
            }
        }
    }
}
