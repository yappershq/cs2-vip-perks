using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.AntiFlash.Configuration;

internal sealed class AntiFlashConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static AntiFlashConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "antiflash.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AntiFlashConfig>(json) ?? new();
    }
}
