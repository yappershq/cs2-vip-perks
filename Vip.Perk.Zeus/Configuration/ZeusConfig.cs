using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Zeus.Configuration;

internal sealed class ZeusConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static ZeusConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip.perk.zeus.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ZeusConfig>(json) ?? new();
    }
}
