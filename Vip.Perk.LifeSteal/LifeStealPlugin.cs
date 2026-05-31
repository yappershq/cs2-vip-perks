using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.LifeSteal;

public sealed class LifeStealPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.LifeSteal";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<LifeStealPlugin> _logger;
    private readonly InterfaceBridge          _bridge;
    private readonly LifeStealPerk            _perk;

    public LifeStealPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<LifeStealPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new LifeStealPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.LifeSteal] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.LifeSteal] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
