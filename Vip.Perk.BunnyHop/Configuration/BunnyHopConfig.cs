using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.BunnyHop.Configuration;

internal sealed class BunnyHopConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("duration")]
    public float Duration { get; set; } = 5.0f;

    [JsonPropertyName("maxSpeed")]
    public float MaxSpeed { get; set; } = 350.0f;

    internal static BunnyHopConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "bunnyhop.json");
        if (!File.Exists(path))
        {
            var def = new BunnyHopConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<BunnyHopConfig>(json) ?? new();
    }
}
