using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.LifeSteal.Configuration;

internal sealed class LifeStealConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static LifeStealConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "lifesteal.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<LifeStealConfig>(json) ?? new();
    }
}
