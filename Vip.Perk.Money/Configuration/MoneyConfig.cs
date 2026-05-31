using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Money.Configuration;

internal sealed class MoneyConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    internal static MoneyConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip.perk.money.json");
        if (!File.Exists(path)) return new();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MoneyConfig>(json) ?? new();
    }
}
