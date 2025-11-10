using GEST.Application.Abstractions;
using GEST.Application.Abstractions.Repositories;
using GEST.Application.Dtos.Webhook;
using GEST.Application.Notifications;
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
    ITimeProvider time,
    IPublishEvent publisher
    ) : IParkingAppService
{
    public async Task HandleEntryAsync(EntryEventDto dto, CancellationToken ct)
    {
        using var transaction = await uow.BeginTransactionAsync(ct);

        try
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

            if (totalSpotAvaliable <= 0)
            {
                await publisher.PublishAsync(new DomainNotification("Parking.Entry.Full", "Estacionamento cheio. Entrada bloqueada."), ct);
                return;
            }

            var vehicle = await vehicleRepo.GetByLicensePlateAsync(dto.License_Plate, ct);

            if (vehicle is null)
            {
                await vehicleRepo.AddAsync(new Vehicle { LicensePlate = dto.License_Plate }, ct);
                await uow.SaveChangesAsync(ct);

                vehicle = await vehicleRepo.GetByLicensePlateAsync(dto.License_Plate, ct);
                if (vehicle is null)
                {
                    await publisher.PublishAsync(new DomainNotification("Parking.Entry.VehiclePersistFail", "Falha ao persistir veículo."), ct);
                    return;
                }
            }

            var (tier, multiplier) = PricingHelper.DecideDynamicPrice(totalParkingSessions, totalSpot);

            ParkingSession session = new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle!.Id,
                EntryTimeUtc = dto.Entry_Time,
                PricingTier = tier,
                Multiplier = multiplier,
                AppliedPricePerHour = 0m,
                Status = ParkingStatus.Active
            };

            await sessionRepo.AddAsync(session, ct);
            await uow.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task HandleParkedAsync(ParkedEventDto dto, CancellationToken ct)
    {
        using var transaction = await uow.BeginTransactionAsync(ct);

        try
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

            var session = await sessionRepo.GetActiveByPlateAsync(dto.License_Plate, ct);
            if (session is null)
            {
                await publisher.PublishAsync(new DomainNotification("Parking.Parked.SessionNotFound", "Sessão ativa não encontrada para a placa."), ct);
                return;
            }

            var spot = await spotRepo.FindByGeoAsync(dto.Lat, dto.Lng, ct);
            if (spot is null)
            {
                await publisher.PublishAsync(new DomainNotification("Parking.Parked.SpotNotFound", "Vaga não encontrada para as coordenadas informadas."), ct);
                return;
            }

            var sector = await sectorRepo.GetByIdAsync(spot.SectorId, ct);
            if (sector is null)
            {
                await publisher.PublishAsync(new DomainNotification("Parking.Parked.SectorNotFound", "Setor não encontrado para a sessão."), ct);
                return;
            }

            await spotRepo.SetOccupiedAsync(spot.Id, dto.License_Plate, ct);
            session.SpotId = spot.Id;
            session.SectorId = sector.Id;
            session.ParkedTimeUtc = time.UtcNow;
            session.AppliedPricePerHour = decimal.Round(sector.BasePrice * session.Multiplier, 2, MidpointRounding.AwayFromZero);

            await uow.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task HandleExitAsync(ExitEventDto dto, CancellationToken ct)
    {
        using var transaction = await uow.BeginTransactionAsync(ct);

        try
        {
            await logRepo.AddAsync(new WebhookEventLog
            {
                EventType = WebhookEventType.EXIT,
                LicensePlate = dto.License_Plate,
                ExitTimeUtc = dto.Exit_Time,
                ReceivedAtUtc = time.UtcNow,
                RawPayloadJson = System.Text.Json.JsonSerializer.Serialize(dto)
            }, ct);

            var session = await sessionRepo.GetActiveByPlateAsync(dto.License_Plate, ct);
            if (session is null)
            {
                await publisher.PublishAsync(new DomainNotification("Parking.Exit.SessionNotFound", "Sessão ativa não encontrada para a placa."), ct);
                return;
            }

            var exitUtc = dto.Exit_Time;
            var amount = PricingHelper.ComputeAmount(session.EntryTimeUtc, exitUtc, session.AppliedPricePerHour);

            await sessionRepo.CloseAsync(session.Id, exitUtc, amount, ct);

            if (session.SpotId is int spotId)
                await spotRepo.FreeAsync(spotId, ct);

            await uow.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
