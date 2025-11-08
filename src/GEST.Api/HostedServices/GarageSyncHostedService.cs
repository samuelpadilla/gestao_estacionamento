using GEST.Application.Services.Garage;

namespace GEST.Api.HostedServices;

public sealed class GarageSyncHostedService(
    ILogger<GarageSyncHostedService> logger,
    IGarageAppService garageSync) : IHostedService
{
    private readonly ILogger<GarageSyncHostedService> _logger = logger;
    private readonly IGarageAppService _garageSync = garageSync;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando sincronização da garagem...");
            await _garageSync.GetAsync(cancellationToken);
            _logger.LogInformation("Sincronização da garagem concluída.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao sincronizar a garagem no startup.");
            // Decide se quer falhar a aplicação ou apenas logar:
            // throw; // se quiser abortar o startup
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
