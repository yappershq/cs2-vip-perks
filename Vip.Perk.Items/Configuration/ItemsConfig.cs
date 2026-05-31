using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Items.Configuration;

internal sealed class ItemsConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("teamItems")]
    public Dictionary<string, string[]> TeamItems { get; set; } = new()
    {
        { "CT", System.Array.Empty<string>() },
        { "T", System.Array.Empty<string>() },
    };

    internal static ItemsConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "items.json");
        if (!File.Exists(path))
        {
            var def = new ItemsConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ItemsConfig>(json) ?? new();
    }
}
