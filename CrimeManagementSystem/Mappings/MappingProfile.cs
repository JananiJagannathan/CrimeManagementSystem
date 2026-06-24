using AutoMapper;
using CrimeManagementSystem.DTOs;
using CrimeManagementSystem.Models;

namespace CrimeManagementSystem.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
            CreateMap<Incident, IncidentResponseDTO>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ReportedByName,
                    opt => opt.MapFrom(src => src.ReportedBy != null ? src.ReportedBy.Name : ""))
                .ForMember(dest => dest.AssignedOfficerName,
                    opt => opt.MapFrom(src => src.AssignedOfficer != null ? src.AssignedOfficer.Name : null));

            
            CreateMap<Officer, OfficerResponseDTO>()
                .ForMember(dest => dest.ActiveIncidentsCount, opt => opt.Ignore());
        }
    }
}