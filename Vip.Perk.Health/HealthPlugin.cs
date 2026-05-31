using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.Health.Configuration;

namespace Vip.Perk.Health;

public sealed class HealthPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Health";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<HealthPlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly HealthConfig          _config;
    private readonly HealthPerk            _perk;

    public HealthPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<HealthPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = HealthConfig.Load(sharpPath);
        _perk   = new HealthPerk(sharedSystem, _logger, _config);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.Health] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.Health] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Health] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
