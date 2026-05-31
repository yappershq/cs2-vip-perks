using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.NoFallDamage.Configuration;

internal sealed class NoFallDamageConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("mode")]
    public int Mode { get; set; } = -1;

    internal static NoFallDamageConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "nofalldamage.json");
        if (!File.Exists(path))
        {
            var def = new NoFallDamageConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<NoFallDamageConfig>(json) ?? new();
    }
}
