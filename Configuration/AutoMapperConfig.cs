using AutoMapper;
using health_information.Models;
using health_information.Models.Dto;

namespace health_information.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
       {
            CreateMap<UserDto, User>().ReverseMap();
            CreateMap<UserReadonlyDto, User>().ReverseMap();
            /*CreateMap<Patient, PatientResponseDto>()
               .ForMember(dest => dest.CreatedBy,
                          opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Username : "Unknown Researcher"));
            CreateMap<Patient, PatientResponseDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy != null && !string.IsNullOrEmpty(src.CreatedBy.Username)
                ? src.CreatedBy.Username : $"Researcher Id: {src.CreatedById}")).ReverseMap();*/

            CreateMap<Patient, PatientResponseDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    (src.CreatedBy != null && !string.IsNullOrEmpty(src.CreatedBy.Username))
                    ? src.CreatedBy.Username
                    : $"Researcher ID: {src.CreatedById}")); 
            CreateMap<PatientCreateDto, Patient>().ReverseMap();
            CreateMap<PatientDto, Patient>().ReverseMap();

            CreateMap<VitalRecord, VitalRecordResponseDto>().ReverseMap();

            CreateMap<VitalRecordCreateDto, VitalRecord>().ReverseMap();
            
            CreateMap<Admin, AdminUserDto>().ReverseMap();
            CreateMap<AdminUserUpsertDto, Admin>().ForMember(dest => dest.PasswordSalt, opt => opt.Ignore()).ForMember(dest => dest.PasswordSalt, opt => opt.Ignore()).ReverseMap();

            CreateMap<Patient, AdminPatientDto>().ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src =>
            src.CreatedBy != null && !string.IsNullOrEmpty(src.CreatedBy.Username)));

            CreateMap<Admin, AdminUserUpsertDto>();
            CreateMap<User, AdminUserDto>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role)).ReverseMap();

        }
    }
}
