using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.DefuseKit.Configuration;

internal sealed class DefuseKitConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static DefuseKitConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "defusekit.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DefuseKitConfig>(json) ?? new();
    }
}
