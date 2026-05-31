using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Zeus.Configuration;

internal sealed class ZeusConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("mode")]
    public int Mode { get; set; } = -1;

    internal static ZeusConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "zeus.json");
        if (!File.Exists(path))
        {
            var def = new ZeusConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ZeusConfig>(json) ?? new();
    }
}
