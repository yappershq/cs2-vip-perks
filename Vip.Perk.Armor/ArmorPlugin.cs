using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.Armor;

public sealed class ArmorPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Armor";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<ArmorPlugin> _logger;
    private readonly InterfaceBridge      _bridge;
    private readonly ArmorPerk            _perk;

    public ArmorPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ArmorPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new ArmorPerk(sharedSystem, _logger);
    }

    public bool Init()
    {
        _perk.Install();
        return true;
    }

    public void PostInit() { }

    public void OnAllModulesLoaded()
    {
        if (!_bridge.ResolveRequired())
        {
            _logger.LogWarning("[Vip.Perk.Armor] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Armor] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
