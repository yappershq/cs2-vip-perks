using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.AntiFlash.Configuration;

namespace Vip.Perk.AntiFlash;

public sealed class AntiFlashPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.AntiFlash";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<AntiFlashPlugin> _logger;
    private readonly InterfaceBridge          _bridge;
    private readonly AntiFlashConfig          _config;
    private readonly AntiFlashPerk            _perk;

    public AntiFlashPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<AntiFlashPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = AntiFlashConfig.Load(sharpPath);
        _perk   = new AntiFlashPerk(sharedSystem, _logger, _config);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.AntiFlash] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.AntiFlash] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.AntiFlash] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
