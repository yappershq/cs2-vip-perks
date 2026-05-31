using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Speed.Configuration;

internal sealed class SpeedConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("multiplier")]
    public float Multiplier { get; set; } = 1.3f;

    internal static SpeedConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "speed.json");
        if (!File.Exists(path))
        {
            var def = new SpeedConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SpeedConfig>(json) ?? new();
    }
}
