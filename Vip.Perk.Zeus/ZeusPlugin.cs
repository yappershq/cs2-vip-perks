using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.Zeus;

public sealed class ZeusPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Zeus";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<ZeusPlugin> _logger;
    private readonly InterfaceBridge     _bridge;
    private readonly ZeusPerk            _perk;

    public ZeusPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ZeusPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new ZeusPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.Zeus] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Zeus] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
