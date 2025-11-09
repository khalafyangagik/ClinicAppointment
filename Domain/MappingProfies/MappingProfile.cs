using AutoMapper;
using Domain.DTOs;
using Domain.Models;

namespace Domain.MappingProfies
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
           
            CreateMap<CreateClinicDto, Clinic>().ReverseMap();
            CreateMap<RegisterDoctorDto, Doctor>().ReverseMap();
            CreateMap<RegisterPatientDto, Patient>().ReverseMap();
            CreateMap<PatientDto, Patient>().ReverseMap();
            CreateMap<CreateNoteDto, Note>().ReverseMap();
            CreateMap<Doctor, DoctorDto>()
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic.Name));
            CreateMap<CreateDoctorDto, Doctor>()
    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
    .ForMember(dest => dest.Speciality, opt => opt.MapFrom(src => src.Speciality))
    .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.ClinicId));

        }
    }
}