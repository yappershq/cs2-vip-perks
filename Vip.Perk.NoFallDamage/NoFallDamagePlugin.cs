using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.NoFallDamage.Configuration;

namespace Vip.Perk.NoFallDamage;

public sealed class NoFallDamagePlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.NoFallDamage";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<NoFallDamagePlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly NoFallDamageConfig          _config;
    private readonly NoFallDamagePerk            _perk;

    public NoFallDamagePlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<NoFallDamagePlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = NoFallDamageConfig.Load(sharpPath);
        _perk   = new NoFallDamagePerk(sharedSystem, _logger);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.NoFallDamage] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.NoFallDamage] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.NoFallDamage] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
