using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.SmokeColor.Configuration;

internal sealed class SmokeColorConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static SmokeColorConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip.perk.smokecolor.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SmokeColorConfig>(json) ?? new();
    }
}
