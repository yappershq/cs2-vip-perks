using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.KillScreen;

public sealed class KillScreenPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.KillScreen";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<KillScreenPlugin> _logger;
    private readonly InterfaceBridge           _bridge;
    private readonly KillScreenPerk            _perk;

    public KillScreenPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<KillScreenPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new KillScreenPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.KillScreen] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.KillScreen] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
