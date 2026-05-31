using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.HitMarker.Configuration;

internal sealed class HitMarkerConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static HitMarkerConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "hitmarker.json");
        if (!File.Exists(path))
        {
            var def = new HitMarkerConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<HitMarkerConfig>(json) ?? new();
    }
}
