using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.PoopGate.Configuration;

internal sealed class PoopGateConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static PoopGateConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "poopgate.json");
        if (!File.Exists(path))
        {
            var def = new PoopGateConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PoopGateConfig>(json) ?? new();
    }
}
