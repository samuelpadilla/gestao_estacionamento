using AutoMapper;
using GEST.Application.Dtos.Garage;
using GEST.Domain.Entities;

namespace GEST.Application.Mapping;

public sealed class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<GarageSectorDto, Sector>()
            .ForMember(d => d.Code, m => m.MapFrom(s => s.Sector))
            .ForMember(d => d.BasePrice, m => m.MapFrom(s => s.BasePrice))
            .ForMember(d => d.MaxCapacity, m => m.MapFrom(s => s.Max_Capacity))
            .ReverseMap();

        CreateMap<GarageSpotDto, Spot>()
            .ForMember(d => d.Sector.Code, m => m.MapFrom(s => s.SectorCode))
            .ForMember(d => d.IsOccupied, m => m.Ignore())
            .ForMember(d => d.CurrentLicensePlate, m => m.Ignore())
            .ForMember(d => d.ParkingSessions, m => m.Ignore())
            .ForMember(d => d.Sector, m => m.Ignore())
            .ReverseMap();
    }
}