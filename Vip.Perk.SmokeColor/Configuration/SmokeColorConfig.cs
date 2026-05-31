using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.SmokeColor.Configuration;

internal sealed class SmokeColorConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("teamColor")]
    public Dictionary<string, int[]> TeamColor { get; set; } = new()
    {
        { "CT", [0, 180, 255] },
        { "T",  [255, 180, 0] },
    };

    internal static SmokeColorConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "smokecolor.json");
        if (!File.Exists(path))
        {
            var def = new SmokeColorConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SmokeColorConfig>(json) ?? new();
    }
}
