using GEST.Api.Endpoints;
using GEST.Application;
using GEST.Infrastructure.Persistence;
using GEST.Infrastructure.Setup;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using System.Text.Json.Serialization;

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

    services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    services.AddProblemDetails(); services.ConfigureHttpJsonOptions(o =>
    {
        // Suporte a DateOnly no JSON
        o.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((doc, ctx, ct) =>
        {
            doc.Info.Title = "GEST Backend API";
            doc.Info.Description = "API para controle de garagem e faturamento.";
            doc.Info.Version = "v1";
            return Task.CompletedTask;
        });
    });

    builder.Services
        .AddInfrastructure(config, env)
        .AddApplication(config);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "GEST API v1");
        });
    }

    app.UseHttpsRedirection();
    app.UseCors();

    app.MapGestEndpoints();

    await SeedData.InitializeAsync(app.Services);

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

public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";
    public override DateOnly Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        => DateOnly.Parse(reader.GetString()!);

    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateOnly value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}