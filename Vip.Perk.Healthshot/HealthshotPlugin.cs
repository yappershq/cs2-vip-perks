using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.Healthshot;

public sealed class HealthshotPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Healthshot";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<HealthshotPlugin> _logger;
    private readonly InterfaceBridge           _bridge;
    private readonly HealthshotPerk            _perk;

    public HealthshotPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<HealthshotPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new HealthshotPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.Healthshot] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Healthshot] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
