using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.Zeus.Configuration;

namespace Vip.Perk.Zeus;

public sealed class ZeusPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Zeus";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<ZeusPlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly ZeusConfig          _config;
    private readonly ZeusPerk            _perk;

    public ZeusPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ZeusPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = ZeusConfig.Load(sharpPath);
        _perk   = new ZeusPerk(sharedSystem, _logger, _config);
    }

    public bool Init()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.Zeus] Disabled via config — skipping.");
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
            _logger.LogWarning("[Vip.Perk.Zeus] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Zeus] Registered.");
    }

    public void Shutdown()
    {
        if (_config.Enabled) _perk.Uninstall();
    }
}
