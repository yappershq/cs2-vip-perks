using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.NoFallDamage.Configuration;

internal sealed class NoFallDamageConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static NoFallDamageConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "nofalldamage.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<NoFallDamageConfig>(json) ?? new();
    }
}
