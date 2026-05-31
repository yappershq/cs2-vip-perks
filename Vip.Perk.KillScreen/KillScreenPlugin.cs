using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.KillScreen.Configuration;

namespace Vip.Perk.KillScreen;

public sealed class KillScreenPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.KillScreen";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<KillScreenPlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly KillScreenConfig          _config;
    private readonly KillScreenPerk            _perk;

    public KillScreenPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<KillScreenPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = KillScreenConfig.Load(sharpPath);
        _perk   = new KillScreenPerk(sharedSystem, _logger, _config);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.KillScreen] Disabled via config — skipping.");
            return true;
        }
        _perk.Install();
        return true;
    }

    public void PostInit() { }

    public void OnAllModulesLoaded()
    {
        if (!_config.Enabled) return;
        if (!_bridge.ResolveRequired())
        {
            _logger.LogWarning("[Vip.Perk.KillScreen] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.KillScreen] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
