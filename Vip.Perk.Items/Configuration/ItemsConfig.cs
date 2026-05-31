using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Items.Configuration;

internal sealed class ItemsConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static ItemsConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "items.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ItemsConfig>(json) ?? new();
    }
}
