using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.SmokeColor;

public sealed class SmokeColorPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.SmokeColor";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<SmokeColorPlugin> _logger;
    private readonly InterfaceBridge           _bridge;
    private readonly SmokeColorPerk            _perk;

    public SmokeColorPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<SmokeColorPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new SmokeColorPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.SmokeColor] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.SmokeColor] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
