using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.Items.Configuration;

namespace Vip.Perk.Items;

public sealed class ItemsPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Items";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<ItemsPlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly ItemsConfig          _config;
    private readonly ItemsPerk            _perk;

    public ItemsPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ItemsPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = ItemsConfig.Load(sharpPath);
        _perk   = new ItemsPerk(sharedSystem, _logger, _config);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.Items] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.Items] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Items] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
