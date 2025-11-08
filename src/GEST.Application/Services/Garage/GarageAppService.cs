using AutoMapper;
using GEST.Application.Abstractions.Gateways;
using GEST.Application.Abstractions.Repositories;
using GEST.Application.Dtos.Garage;
using GEST.Domain.Abstractions;

namespace GEST.Application.Services.Garage;

public interface IGarageAppService
{
    Task<GarageConfigDto> GetAsync(CancellationToken ct);
    Task<GarageStateDto> GetStateAsync(CancellationToken ct);
}

public sealed class GarageAppService(
    IGarageGateway gateway,
    ISectorRepository sectorRepo,
    ISpotRepository spotRepo,
    IMapper mapper,
    IUnitOfWork uow
    ) : IGarageAppService
{
    public async Task<GarageConfigDto> GetAsync(CancellationToken ct)
    {        
        var sectors = await sectorRepo.GetAllAsync(ct);
        var spots = await spotRepo.GetAllAsync(ct);
        
        return new GarageConfigDto
        {
            Garage = mapper.Map<List<GarageSectorDto>>(sectors.ToList()),
            Spots = mapper.Map<List<GarageSpotDto>>(spots.ToList())
        };
    }

    public async Task<GarageStateDto> GetStateAsync(CancellationToken ct)
    {
        var sectors = await sectorRepo.GetAllAsync(ct);
        var spots = await spotRepo.GetAllAsync(ct);

        var state = new GarageStateDto
        {
            Sectors = new List<GarageSectorStateDto>()
        };

        foreach (var sector in sectors)
        {
            var sectorSpots = spots.Where(s => s.SectorCode == sector.Code).ToList();

            var occupiedSpots = sectorSpots.Count(s => s.IsOccupied);

            var sectorState = new GarageSectorStateDto
            {
                SectorCode = sector.Code,
                Capacity = sectorSpots.Count,
                Occupied = occupiedSpots,
                Available = sectorSpots.Count - occupiedSpots
            };
            state.Sectors.Add(sectorState);
        }

        return state;
    }
}
