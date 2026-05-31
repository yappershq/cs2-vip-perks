using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.KillScreen.Configuration;

internal sealed class KillScreenConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static KillScreenConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip.perk.killscreen.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<KillScreenConfig>(json) ?? new();
    }
}
