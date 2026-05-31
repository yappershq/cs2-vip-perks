using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Gravity.Configuration;

internal sealed class GravityConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("gravityScale")]
    public float GravityScale { get; set; } = 0.9f;

    internal static GravityConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "gravity.json");
        if (!File.Exists(path))
        {
            var def = new GravityConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GravityConfig>(json) ?? new();
    }
}
