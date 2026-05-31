using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Armor.Configuration;

internal sealed class ArmorConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static ArmorConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip.perk.armor.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ArmorConfig>(json) ?? new();
    }
}
