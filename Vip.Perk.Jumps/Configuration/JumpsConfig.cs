using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Jumps.Configuration;

internal sealed class JumpsConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("teamExtraJumps")]
    public Dictionary<string, int> TeamExtraJumps { get; set; } = new()
    {
        { "CT", 1 },
        { "T", 1 },
    };

    internal static JumpsConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "jumps.json");
        if (!File.Exists(path))
        {
            var def = new JumpsConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<JumpsConfig>(json) ?? new();
    }
}
