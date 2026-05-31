using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.Speed;

public sealed class SpeedPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Speed";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<SpeedPlugin> _logger;
    private readonly InterfaceBridge      _bridge;
    private readonly SpeedPerk            _perk;

    public SpeedPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<SpeedPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new SpeedPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.Speed] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Speed] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
