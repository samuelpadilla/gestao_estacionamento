using Gest.Simulador.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = "https://localhost:7004"; // ajuste conforme necessário
var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    PropertyNameCaseInsensitive = true
};

Console.WriteLine($"GET {baseUrl}/garage");
GarageConfigDto? config;
try
{
    using var resp = await http.GetAsync("/garage");
    resp.EnsureSuccessStatusCode();
    config = await resp.Content.ReadFromJsonAsync<GarageConfigDto>(jsonOptions);
    if (config is null)
    {
        Console.WriteLine("Configuração inválida.");
        return;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao obter /garage: {ex.Message}");
    return;
}

Console.WriteLine($"Garage configuration loaded. Sectors: {config.Garage.Count} Spots: {config.Spots.Count}");

string GeneratePlate()
{
    const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    var rnd = Random.Shared;
    return string.Concat(
        letters[rnd.Next(letters.Length)],
        letters[rnd.Next(letters.Length)],
        letters[rnd.Next(letters.Length)],
        rnd.Next(0, 9999).ToString("D4")
    );
}

async Task PostAsync(object payload, string label)
{
    try
    {
        var resp = await http.PostAsJsonAsync("/webhook", payload, jsonOptions);
        if (resp.IsSuccessStatusCode)
        {
            Console.WriteLine($"[{DateTime.UtcNow:O}] {label} enviado com sucesso");
            return;
        }

        var body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"[{DateTime.UtcNow:O}] Falha {label}: {(int)resp.StatusCode} {resp.ReasonPhrase}");

        // Tenta decodificar ValidationProblem (Formatação usada pela API)
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("errors", out var errorsElem) && errorsElem.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errorsElem.EnumerateObject())
                {
                    var key = prop.Name;
                    var arr = prop.Value;
                    if (arr.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var msg in arr.EnumerateArray())
                        {
                            Console.WriteLine($" - {key}: {msg.GetString()}");
                        }
                    }
                    else if (arr.ValueKind == JsonValueKind.String)
                    {
                        Console.WriteLine($" - {key}: {arr.GetString()}");
                    }
                }
                return; // já exibido estruturado
            }
        }
        catch (Exception parseEx)
        {
            Console.WriteLine($" (Falha ao parsear JSON de erro: {parseEx.Message})");
        }

        // fallback: corpo bruto
        if (!string.IsNullOrWhiteSpace(body))
            Console.WriteLine(" Corpo:");
        Console.WriteLine(" " + body.Replace('\n', ' ').Trim());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:O}] Erro ao enviar {label}: {ex.Message}");
    }
}

async Task SimulateSequenceAsync(int index, string? plate = null)
{
    plate ??= GeneratePlate();
    Console.WriteLine($"Iniciando sequência para {plate}...");

    // Duração aleatória entre 10 minutos e 6 horas
    var durationMinutes = Random.Shared.Next(10, 361); // 10..360
    var exitTime = DateTime.UtcNow; // saída no momento do envio
    var entryTime = exitTime.AddMinutes(-durationMinutes);

    var spot = config.Spots.FirstOrDefault();
    if (spot is null)
    {
        Console.WriteLine("Nenhuma vaga disponível para simulação PARKED.");
        return;
    }

    var entry = new EntryWebhook
    {
        LicensePlate = plate,
        EntryTime = entryTime,
        EventType = "ENTRY"
    };

    var parked = new ParkedWebhook
    {
        LicensePlate = plate,
        Lat = spot.Lat,
        Lng = spot.Lng,
        EventType = "PARKED"
    };

    var exit = new ExitWebhook
    {
        LicensePlate = plate,
        ExitTime = exitTime,
        EventType = "EXIT"
    };

    await PostAsync(entry, "ENTRY");
    await Task.Delay(300); // pequeno intervalo
    await PostAsync(parked, "PARKED");
    await Task.Delay(300);
    await PostAsync(exit, "EXIT");

    Console.WriteLine($"Sequência concluída para {plate}. (duração ~{durationMinutes} min)");
}

int sequences = 3; // config.Spots.Count();
for (int i = 0; i < sequences; i++)
{
    await SimulateSequenceAsync(i);
}

Console.WriteLine("Simulação finalizada.");
Console.ReadKey();
