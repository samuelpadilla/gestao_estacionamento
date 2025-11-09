using GEST.Application.Abstractions;
using GEST.Application.Abstractions.Repositories;
using GEST.Application.Dtos.Webhook;
using GEST.Application.Services.Pricing;
using GEST.Domain.Abstractions;
using GEST.Domain.Entities;
using GEST.Domain.Enums;

namespace GEST.Application.Services.Parking;

public interface IParkingAppService
{
    Task HandleEntryAsync(EntryEventDto dto, CancellationToken ct);
    Task HandleParkedAsync(ParkedEventDto dto, CancellationToken ct);
    Task HandleExitAsync(ExitEventDto dto, CancellationToken ct);
}

public sealed class ParkingAppService(
    ISectorRepository sectorRepo,
    ISpotRepository spotRepo,
    IVehicleRepository vehicleRepo,
    IParkingSessionRepository sessionRepo,
    IWebhookEventLogRepository logRepo,
    IUnitOfWork uow,
    ITimeProvider time
    ) : IParkingAppService
{
    public async Task HandleEntryAsync(EntryEventDto dto, CancellationToken ct)
    {
        await uow.ExecuteInTransactionAsync(async _ =>
        {
            await logRepo.AddAsync(new WebhookEventLog
            {
                EventType = WebhookEventType.ENTRY,
                LicensePlate = dto.License_Plate,
                EntryTimeUtc = dto.Entry_Time,
                ReceivedAtUtc = time.UtcNow,
                RawPayloadJson = System.Text.Json.JsonSerializer.Serialize(dto)
            }, ct);

            var totalSpot = await spotRepo.CountAllAsync(ct);
            var totalParkingSessions = await sessionRepo.CountActiveAsync(ct);
            var totalSpotAvaliable = totalSpot - totalParkingSessions;

            if (totalSpotAvaliable == 0)
                throw new InvalidOperationException("Estacionamento cheio. Entrada bloqueada.");

            var vehicle = vehicleRepo.GetByLicensePlateAsync(dto.License_Plate, ct);

            if (vehicle is null)
            {
                await vehicleRepo.AddAsync(new Vehicle { LicensePlate = dto.License_Plate }, ct);
                await uow.SaveChangesAsync(ct);

                vehicle = vehicleRepo.GetByLicensePlateAsync(dto.License_Plate, ct);
            }

            var (tier, multiplier) = PricingHelper.DecideDynamicPrice(totalSpotAvaliable, totalSpot);

            ParkingSession session = new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                EntryTimeUtc = dto.Entry_Time,
                PricingTier = tier,
                AppliedPricePerHour = 0m,
                Status = ParkingStatus.Active
            };

            await sessionRepo.AddAsync(session, ct);
            await uow.SaveChangesAsync(ct);
        }, ct: ct);
    }

    public async Task HandleParkedAsync(ParkedEventDto dto, CancellationToken ct)
    {
        await uow.ExecuteInTransactionAsync(async _ =>
        {
            await logRepo.AddAsync(new WebhookEventLog
            {
                EventType = WebhookEventType.PARKED,
                LicensePlate = dto.License_Plate,
                Lat = dto.Lat,
                Lng = dto.Lng,
                ReceivedAtUtc = time.UtcNow,
                RawPayloadJson = System.Text.Json.JsonSerializer.Serialize(dto)
            }, ct);

            var session = await sessionRepo.GetActiveByPlateAsync(dto.License_Plate, ct)
                ?? throw new InvalidOperationException("Sessão ativa não encontrada para a placa.");

            var spot = await spotRepo.FindByGeoAsync(dto.Lat, dto.Lng, ct)
                ?? throw new InvalidOperationException("Vaga não encontrada para as coordenadas informadas.");

            var sector = await sectorRepo.GetByIdAsync(session.SectorId!.Value, ct)
                ?? throw new InvalidOperationException("Setor não encontrado para a sessão.");

            await spotRepo.SetOccupiedAsync(spot.Id, dto.License_Plate, ct);
            session.SpotId = spot.Id;
            session.ParkedTimeUtc = time.UtcNow;
            session.AppliedPricePerHour = decimal.Round(sector.BasePrice * session.Multiplier, 2, MidpointRounding.AwayFromZero);

            await uow.SaveChangesAsync(ct);
        }, ct: ct);
    }

    public async Task HandleExitAsync(ExitEventDto dto, CancellationToken ct)
    {
        await uow.ExecuteInTransactionAsync(async _ =>
        {
            await logRepo.AddAsync(new WebhookEventLog
            {
                EventType = WebhookEventType.EXIT,
                LicensePlate = dto.License_Plate,
                ExitTimeUtc = dto.Exit_Time,
                ReceivedAtUtc = time.UtcNow,
                RawPayloadJson = System.Text.Json.JsonSerializer.Serialize(dto)
            }, ct);

            var session = await sessionRepo.GetActiveByPlateAsync(dto.License_Plate, ct)
                          ?? throw new InvalidOperationException("Sessão ativa não encontrada para a placa.");

            var exitUtc = dto.Exit_Time;
            var amount = PricingHelper.ComputeAmount(session.EntryTimeUtc, exitUtc, session.AppliedPricePerHour);

            await sessionRepo.CloseAsync(session.Id, exitUtc, amount, ct);

            if (session.SpotId is int spotId)
                await spotRepo.FreeAsync(spotId, ct);

            await uow.SaveChangesAsync(ct);
        }, ct: ct);
    }
}
