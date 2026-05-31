using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Armor.Configuration;

internal sealed class ArmorConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("armorValue")]
    public int ArmorValue { get; set; } = 50;

    internal static ArmorConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "armor.json");
        if (!File.Exists(path))
        {
            var def = new ArmorConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ArmorConfig>(json) ?? new();
    }
}
