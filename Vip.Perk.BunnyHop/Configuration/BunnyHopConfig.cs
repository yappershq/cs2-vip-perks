using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.BunnyHop.Configuration;

internal sealed class BunnyHopConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static BunnyHopConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip.perk.bunnyhop.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<BunnyHopConfig>(json) ?? new();
    }
}
