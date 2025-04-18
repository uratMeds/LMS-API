namespace LMS_API.Profiles;

using AutoMapper;
using LMS_API.DTOs;
using LMS_API.Models;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LeaveRequest, LeaveRequestDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FullName))
                .ForMember(dest => dest.LeaveType, opt => opt.MapFrom(src => src.LeaveType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateLeaveRequestDto, LeaveRequest>();
        }
    }