using AutoMapper;
using health_information.Models;
using health_information.Models.Dto;
using System.Security.Cryptography;

namespace health_information.Services
{
    public class PatientService : IPatientService
    {
        private readonly IHealthReposiotory<Patient> _patientRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public PatientService(IHealthReposiotory<Patient> patientRepo, IMapper mapper, IConfiguration config)
        {
            _patientRepo = patientRepo;
            _mapper = mapper;
            _config = config;
        }
        public async Task<bool> CreateAsync(PatientCreateDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto, $"The argument {nameof(dto)} is null");

            // 1. Automated unique patient study id generation
            if (string.IsNullOrEmpty(dto.PatientStudyId))
            {
                bool isUnique = false;
                string generatedId = string.Empty;

                // Loop until a unique patient study id is generated
                while (!isUnique)
                {
                    int randomNumber = RandomNumberGenerator.GetInt32(1000, 9999);
                    generatedId = $"SUBJ-{DateTime.Now.Year}-{randomNumber}";

                    // Verify it's not already in the database
                    var checkDuplicate = await _patientRepo.GetAsync(p => p.PatientStudyId == generatedId);
                    if (checkDuplicate == null)
                    {
                        isUnique = true;
                    }
                }
                //  Assign outside the loop once it is proven unique!
                dto.PatientStudyId = generatedId;
            }
            else
            {
                
                var existingPatient = await _patientRepo.GetAsync(p => p.PatientStudyId == dto.PatientStudyId);

                if (existingPatient != null)
                {
                    throw new ArgumentException($"The patient with the study id '{dto.PatientStudyId}' already exists.");
                }
            }

            
            Patient patient = _mapper.Map<Patient>(dto);
            await _patientRepo.CreateAsync(patient);
            return true;
        }
            

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Patient Id");

            var patient = await _patientRepo.GetAsync(p => p.Id == id);
            if(patient == null) 
                throw new Exception($"Patient with id {id} does not exist");

            return await _patientRepo.DeleteAsync(patient);
        }

        public async Task<List<PatientResponseDto>> GetAllAsync()
        {
           var patient =  await _patientRepo.GetAllByFilterAsync(p => true, IncludeProperties: "CreatedBy");
            
            return _mapper.Map<List<PatientResponseDto>>(patient);
        }

        public async Task<PatientResponseDto?> GetByIdAsync(int id)
        {
            var patient = await _patientRepo.GetAsync(
                filter: p => p.Id == id, useNoTracking: true,
                IncludeProperties: "CreatedBy");

            if (patient == null)
                return null;

                return _mapper.Map<PatientResponseDto>(patient);
        }

        public async  Task<PatientResponseDto?> GetByStudyIdAsync(string studyId)
        {
            if (string.IsNullOrWhiteSpace(studyId))
                return null;

            var patient = await _patientRepo.GetAsync(
                filter: p => p.PatientStudyId == studyId, useNoTracking: true,
                IncludeProperties: "CreatedBy");

            if(patient == null)
            {

                return null;
            }

            return  _mapper.Map<PatientResponseDto>(patient);
        }

        public async Task<PatientResponseDto> GetpatientWithVitalsAsync(int id)
        {
            var profile = await _patientRepo.GetAsync(
                filter: p => p.Id == id, useNoTracking: true,
                IncludeProperties: "CreatedBy, VitalRecords");

            if(profile == null)
                return null;

            return _mapper.Map<PatientResponseDto>(profile);
        }

        public async Task<bool> UpdateAsync(PatientDto dto)
        {
           var patient = await _patientRepo.GetAsync(p => p.Id == dto.Id);
            if (patient == null)
                throw new Exception($"Patient with id {dto.Id} does not exist");
            _mapper.Map(dto, patient);
             await _patientRepo.UpdateAsync(patient);
            return true;
        }
    }
}
