using AspNetCoreRateLimit;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;

SelfLog.Enable(msg =>
{
    Directory.CreateDirectory("logs");
    File.AppendAllText("logs/serilog-selflog.txt", msg + Environment.NewLine);
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/startup-.log", rollingInterval: RollingInterval.Day, shared: true)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var services = builder.Services;
    var config = builder.Configuration;
    var env = builder.Environment;

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    if (env.IsDevelopment())
        builder.Configuration.AddUserSecrets<Program>(optional: true);

    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration)
          .ReadFrom.Services(services)
          .Enrich.FromLogContext()
          .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
          .WriteTo.Console()
          .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, shared: true);
    });

    services.AddCors();
    services.AddProblemDetails();
    services.AddMemoryCache();
    services.AddOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    app.UseIpRateLimiting();

    Log.Information("API iniciando Kestrel...");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API encerrada por falha inesperada durante a inicialização.");
}
finally
{
    await Log.CloseAndFlushAsync();
}