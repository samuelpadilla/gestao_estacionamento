using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var baseUrl = "https://localhost:7004";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Cancel requested, shutting down...");
    e.Cancel = true;
    cts.Cancel();
};

var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    PropertyNameCaseInsensitive = true
};

// 1) GET /garage
Console.WriteLine($"GET {baseUrl}/garage");
GarageConfigDto? config;
try
{
    using var resp = await http.GetAsync("/garage", cts.Token);
    if (!resp.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to GET /garage: {(int)resp.StatusCode} {resp.ReasonPhrase}");
        return;
    }

    var stream = await resp.Content.ReadAsStreamAsync(cts.Token);
    config = await JsonSerializer.DeserializeAsync<GarageConfigDto>(stream, jsonOptions, cts.Token);
    if (config is null)
    {
        Console.WriteLine("Received empty or invalid configuration from /garage.");
        return;
    }
}
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
    Console.WriteLine("Operation cancelled.");
    return;
}
catch (Exception ex)
{
    Console.WriteLine($"Error fetching /garage: {ex.Message}");
    return;
}

Console.WriteLine("Garage configuration loaded");
Console.WriteLine($"  Sectors: {config.Garage?.Count ?? 0}");
Console.WriteLine($"  Spots: {config.Spots?.Count ?? 0}");

// --- Bulk generate 100 ENTRY events for sector A ---
const int entriesToGenerate = 100;
var spotsInA = config.Spots?.Where(s => string.Equals(s.Sector, "A", StringComparison.OrdinalIgnoreCase)).ToList()
               ?? new List<GarageSpotDto>();

if (spotsInA.Count == 0)
{
    Console.WriteLine("No spots found in sector A. Skipping bulk ENTRY generation.");
}
else
{
    Console.WriteLine($"Generating {entriesToGenerate} ENTRY events for sector A...");
    for (int i = 0; i < entriesToGenerate && !cts.IsCancellationRequested; i++)
    {
        var license = GeneratePlate(new Random());
        var entry = new EntryWebhook
        {
            LicensePlate = license,
            EntryTime = DateTime.UtcNow,
            EventType = "ENTRY",
        };

        try
        {
            var postResp = await http.PostAsJsonAsync("/webhook", entry, jsonOptions, cts.Token);
            if (postResp.IsSuccessStatusCode)
            {
                Console.WriteLine($"[{DateTime.UtcNow:O}] Bulk ENTRY #{i + 1} sent: {entry.LicensePlate}");
            }
            else
            {
                Console.WriteLine($"[{DateTime.UtcNow:O}] Failed bulk ENTRY #{i + 1}: {(int)postResp.StatusCode} {postResp.ReasonPhrase}");
                var body = await postResp.Content.ReadAsStringAsync(cts.Token);
                if (!string.IsNullOrEmpty(body)) Console.WriteLine($"  Response body: {body}");
            }
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow:O}] Exception sending bulk ENTRY #{i + 1}: {ex.Message}");
        }

        // small pause to avoid overwhelming the receiver (adjustable)
        try { await Task.Delay(50, cts.Token); } catch (OperationCanceledException) { break; }
    }

    Console.WriteLine("Bulk ENTRY generation finished.");
}

// 2) Start sending events periodically (cycle through ENTRY -> PARKED -> EXIT)
var rnd = new Random();
var intervalSeconds = 5;
Console.WriteLine($"Sending events every {intervalSeconds} second(s). Press Ctrl+C to stop.");

int eventCounter = 0;
while (!cts.IsCancellationRequested)
{
    eventCounter++;
    var ev = CreateSimulatedWebhookEvent(config, rnd, eventCounter);

    try
    {
        var postResp = await http.PostAsJsonAsync("/webhook", ev, jsonOptions, cts.Token);
        if (postResp.IsSuccessStatusCode)
        {
            Console.WriteLine($"[{DateTime.UtcNow:O}] Sent event #{eventCounter}: {GetEventType(ev)} (status {(int)postResp.StatusCode})");
        }
        else
        {
            Console.WriteLine($"[{DateTime.UtcNow:O}] Failed to send event #{eventCounter}: {(int)postResp.StatusCode} {postResp.ReasonPhrase}");
            var body = await postResp.Content.ReadAsStringAsync(cts.Token);
            if (!string.IsNullOrEmpty(body)) Console.WriteLine($"  Response body: {body}");
        }
    }
    catch (OperationCanceledException) when (cts.IsCancellationRequested)
    {
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:O}] Exception sending event: {ex.Message}");
    }

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cts.Token);
    }
    catch (OperationCanceledException) when (cts.IsCancellationRequested)
    {
        break;
    }
}

Console.WriteLine("Simulator stopped.");

static object CreateSimulatedWebhookEvent(GarageConfigDto config, Random rnd, int seq)
{
    // Cycle through three event types so all examples are produced predictably
    var cycle = seq % 3;
    var license = GeneratePlate(rnd);

    // choose a spot if available for PARKED events
    GarageSpotDto? spot = null;
    if (config.Spots is { Count: > 0 })
        spot = config.Spots[rnd.Next(config.Spots.Count)];

    return cycle switch
    {
        1 => new EntryWebhook
        {
            LicensePlate = license,
            EntryTime = DateTime.UtcNow,
            EventType = "ENTRY"
        },
        2 => new ParkedWebhook
        {
            LicensePlate = license,
            Lat = spot?.Lat ?? (-23.561684 + (rnd.NextDouble() - 0.5) * 0.001),
            Lng = spot?.Lng ?? (-46.655981 + (rnd.NextDouble() - 0.5) * 0.001),
            EventType = "PARKED"
        },
        _ => new ExitWebhook
        {
            LicensePlate = license,
            ExitTime = DateTime.UtcNow,
            EventType = "EXIT"
        }
    };
}

static string GetEventType(object ev)
{
    return ev switch
    {
        EntryWebhook e => e.EventType,
        ParkedWebhook p => p.EventType,
        ExitWebhook x => x.EventType,
        _ => "unknown"
    };
}

static string GeneratePlate(Random rnd)
{
    const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    return string.Concat(
        letters[rnd.Next(letters.Length)],
        letters[rnd.Next(letters.Length)],
        letters[rnd.Next(letters.Length)],
        rnd.Next(1, 9999).ToString("D4")
    );
}

// Webhook payload shapes required by receiver (snake_case)
internal sealed class EntryWebhook
{
    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("entry_time")]
    public DateTime EntryTime { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}

internal sealed class ParkedWebhook
{
    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}

internal sealed class ExitWebhook
{
    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("exit_time")]
    public DateTime ExitTime { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}

internal class GarageConfigDto
{
    [JsonPropertyName("garage")]
    public List<GarageSectorDto> Garage { get; set; } = [];
    [JsonPropertyName("spots")]
    public List<GarageSpotDto> Spots { get; set; } = [];
}

internal class GarageSectorDto
{
    [JsonPropertyName("sector")]
    public string Sector { get; set; } = default!;
    [JsonPropertyName("basePrice")]
    public decimal BasePrice { get; set; }
    [JsonPropertyName("max_capacity")]
    public int Max_Capacity { get; set; }
}

internal class GarageSpotDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("sector")]
    public string Sector { get; set; } = default!;
    [JsonPropertyName("lat")]
    public double Lat { get; set; }
    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}
