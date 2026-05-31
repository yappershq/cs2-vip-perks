using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.DefuseKit.Configuration;

internal sealed class DefuseKitConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("usageLimit")]
    public int UsageLimit { get; set; } = -1;

    internal static DefuseKitConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "defusekit.json");
        if (!File.Exists(path))
        {
            var def = new DefuseKitConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DefuseKitConfig>(json) ?? new();
    }
}
