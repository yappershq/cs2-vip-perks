using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vip.Perk.Money.Configuration;

internal sealed class MoneyConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("bonusAmount")]
    public int BonusAmount { get; set; } = 1000;

    internal static MoneyConfig Load(string sharpPath)
    {
        var path = Path.Combine(sharpPath, "configs", "vip", "perks", "money.json");
        if (!File.Exists(path))
        {
            var def = new MoneyConfig();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true }));
            return def;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MoneyConfig>(json) ?? new();
    }
}
