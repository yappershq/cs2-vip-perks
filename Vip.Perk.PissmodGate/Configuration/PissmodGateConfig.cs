using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.PissmodGate.Configuration;

internal sealed class PissmodGateConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static PissmodGateConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "pissmodgate.json");
        if (!File.Exists(path))
        {
            var def = new PissmodGateConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PissmodGateConfig>(json) ?? new();
    }
}
