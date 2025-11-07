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
            CreateMap<CreateDoctorDto,Doctor>().ReverseMap();
            CreateMap<RegisterPatientDto, Patient>().ReverseMap();

        }
    }
}