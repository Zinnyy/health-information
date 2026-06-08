using health_information.Models.Dto;

namespace health_information.Services
{
    public interface IPatientService
    {
        //Register a new patient
        Task<bool> CreateAsync(PatientCreateDto dto);
        Task<PatientResponseDto?> GetByIdAsync(int id);
        Task<List<PatientResponseDto>> GetAllAsync();
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(PatientDto dto);
        Task<PatientResponseDto?> GetByStudyIdAsync(string studyId);
        Task<PatientResponseDto> GetpatientWithVitalsAsync(int id);

    }
}
