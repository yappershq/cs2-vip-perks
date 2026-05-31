using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.Gravity.Configuration;

namespace Vip.Perk.Gravity;

public sealed class GravityPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Gravity";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<GravityPlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly GravityConfig          _config;
    private readonly GravityPerk            _perk;

    public GravityPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<GravityPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = GravityConfig.Load(sharpPath);
        _perk   = new GravityPerk(sharedSystem, _logger, _config);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.Gravity] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.Gravity] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Gravity] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
