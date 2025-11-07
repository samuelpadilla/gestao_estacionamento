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
            // log
            await logRepo.AddAsync(new WebhookEventLog
            {
                EventType = WebhookEventType.ENTRY,
                LicensePlate = dto.License_Plate,
                EntryTimeUtc = dto.Entry_Time,
                ReceivedAtUtc = time.UtcNow,
                RawPayloadJson = System.Text.Json.JsonSerializer.Serialize(dto)
            }, ct);

            // business
            // Para ENTRY, vamos exigir setor (defina sua regra; aqui assumimos único setor quando só há um)
            // Você pode inferir setor a partir da distribuição de vagas; para simplicidade, usamos o primeiro setor.
            var sectorCandidates = new[] { "A" }; // adapte: injete configuração/lookup
            var sector = await sectorRepo.GetAsync(sectorCandidates[0], ct)
                         ?? throw new InvalidOperationException("Setor não encontrado.");

            var occupied = await spotRepo.CountOccupiedAsync(sector.Code, ct);
            if (occupied >= sector.MaxCapacity)
                throw new InvalidOperationException("Estacionamento cheio. Entrada bloqueada.");

            await vehicleRepo.EnsureAsync(dto.License_Plate, ct);

            var (tier, price) = PricingHelper.DecideDynamicPrice(occupied, sector.MaxCapacity, sector.BasePrice);

            var session = new ParkingSession
            {
                Id = Guid.NewGuid(),
                LicensePlate = dto.License_Plate,
                SectorCode = sector.Code,
                EntryTimeUtc = dto.Entry_Time,
                AppliedPricePerHour = decimal.Round(price, 2, MidpointRounding.AwayFromZero),
                PricingTier = tier,
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

            // Localiza vaga pelo geo dentro do mesmo setor (heurística do simulador)
            var spot = await spotRepo.FindByGeoAsync(session.SectorCode, dto.Lat, dto.Lng, ct)
                       ?? throw new InvalidOperationException("Vaga não encontrada para as coordenadas informadas.");

            // marca vaga ocupada e seta parkedTime
            await spotRepo.SetOccupiedAsync(spot.Id, dto.License_Plate, ct);
            session.SpotId = spot.Id;
            session.ParkedTimeUtc = time.UtcNow;

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
