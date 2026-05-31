using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.KillScreen.Configuration;

internal sealed class KillScreenConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("teamFadeColor")]
    public Dictionary<string, int[]> TeamFadeColor { get; set; } = new()
    {
        { "CT", [255, 0, 0, 180] },
        { "T",  [0, 0, 255, 180] },
    };

    internal static KillScreenConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "killscreen.json");
        if (!File.Exists(path))
        {
            var def = new KillScreenConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<KillScreenConfig>(json) ?? new();
    }
}
