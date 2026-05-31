using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.DefuseKit.Configuration;

namespace Vip.Perk.DefuseKit;

public sealed class DefuseKitPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.DefuseKit";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<DefuseKitPlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly DefuseKitConfig          _config;
    private readonly DefuseKitPerk            _perk;

    public DefuseKitPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<DefuseKitPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = DefuseKitConfig.Load(sharpPath);
        _perk   = new DefuseKitPerk(sharedSystem, _logger);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.DefuseKit] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.DefuseKit] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.DefuseKit] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
