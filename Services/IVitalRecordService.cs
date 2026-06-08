using health_information.Models.Dto;

namespace health_information.Services
{
    public interface IVitalRecordService
    {
        Task<bool> AddRecordAsync(VitalRecordCreateDto dto);
        Task<List<VitalRecordResponseDto>> GetRecordsByPatientIdAsync(int PaientId);
        Task<bool> DeleteRecordAsync(int id);
    }
}
