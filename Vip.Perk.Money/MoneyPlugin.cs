using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.Money.Configuration;

namespace Vip.Perk.Money;

public sealed class MoneyPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Money";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<MoneyPlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly MoneyConfig          _config;
    private readonly MoneyPerk            _perk;

    public MoneyPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<MoneyPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = MoneyConfig.Load(sharpPath);
        _perk   = new MoneyPerk(sharedSystem, _logger);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.Money] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.Money] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Money] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
