using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.AntiFlash;

public sealed class AntiFlashPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.AntiFlash";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<AntiFlashPlugin> _logger;
    private readonly InterfaceBridge          _bridge;
    private readonly AntiFlashPerk            _perk;

    public AntiFlashPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<AntiFlashPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new AntiFlashPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.AntiFlash] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.AntiFlash] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
