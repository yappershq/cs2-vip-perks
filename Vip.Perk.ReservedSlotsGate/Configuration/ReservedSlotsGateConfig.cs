using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.ReservedSlotsGate.Configuration;

internal sealed class ReservedSlotsGateConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static ReservedSlotsGateConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "reservedslotsgate.json");
        if (!File.Exists(path))
        {
            var def = new ReservedSlotsGateConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ReservedSlotsGateConfig>(json) ?? new();
    }
}
