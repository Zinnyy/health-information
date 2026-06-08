using AutoMapper;
using health_information.Models;
using health_information.Models.Dto;

namespace health_information.Services
{
    public class VitalRecordService : IVitalRecordService
    {
        private readonly IHealthReposiotory<VitalRecord> _vitalRecordRepo;
        private readonly IHealthReposiotory<Patient> _patientRepo;
        private readonly IMapper _mapper;
        public VitalRecordService(IHealthReposiotory<VitalRecord> vitalRepo, IMapper mapper, IHealthReposiotory<Patient> patientRepo)
        {
            _vitalRecordRepo = vitalRepo;
            _mapper = mapper;
            _patientRepo = patientRepo;
        }
        public async Task<bool> AddRecordAsync(VitalRecordCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, nameof(dto));
            var patient = await _patientRepo.GetAsync(p => p.Id == dto.PatientId);

            if(patient == null)
            {
                throw new Exception($"Cannot add Vitals. Patient with id {dto.PatientId} not found.");
            }

            VitalRecord record = _mapper.Map<VitalRecord>(dto);
            record.DateRecorded = DateTime.UtcNow;

            await _vitalRecordRepo.CreateAsync(record);
            return true;
        }

        public async Task<bool> DeleteRecordAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid record Id." );
            var record = await _vitalRecordRepo.GetAsync(v => v.Id == id);
            if(record == null)
                throw new Exception($"Vital record with id {id} not found.");

            return await _vitalRecordRepo.DeleteAsync(record);
        }

        public async Task<List<VitalRecordResponseDto>> GetRecordsByPatientIdAsync(int PaientId)
        {
            var records = await _vitalRecordRepo.GetAllByFilterAsync(v => v.PatientId == PaientId);
            return _mapper.Map<List<VitalRecordResponseDto>>(records);
        }
    }
}
