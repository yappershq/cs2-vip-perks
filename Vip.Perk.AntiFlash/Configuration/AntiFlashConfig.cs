using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.AntiFlash.Configuration;

internal sealed class AntiFlashConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("mode")]
    public int Mode { get; set; } = 1;

    internal static AntiFlashConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "antiflash.json");
        if (!File.Exists(path))
        {
            var def = new AntiFlashConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AntiFlashConfig>(json) ?? new();
    }
}
