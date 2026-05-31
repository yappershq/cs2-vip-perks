using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.WeaponMenuGate.Configuration;

internal sealed class WeaponMenuGateConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static WeaponMenuGateConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "weaponmenugate.json");
        if (!File.Exists(path))
        {
            var def = new WeaponMenuGateConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<WeaponMenuGateConfig>(json) ?? new();
    }
}
