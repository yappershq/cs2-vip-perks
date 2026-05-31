using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.LifeSteal.Configuration;

internal sealed class LifeStealConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("percent")]
    public float Percent { get; set; } = 30.0f;

    internal static LifeStealConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "lifesteal.json");
        if (!File.Exists(path))
        {
            var def = new LifeStealConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<LifeStealConfig>(json) ?? new();
    }
}
