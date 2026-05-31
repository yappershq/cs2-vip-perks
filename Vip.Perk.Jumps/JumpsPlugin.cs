using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.Jumps;

public sealed class JumpsPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.Jumps";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<JumpsPlugin> _logger;
    private readonly InterfaceBridge      _bridge;
    private readonly JumpsPerk            _perk;

    public JumpsPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<JumpsPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new JumpsPerk(sharedSystem, _logger);
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
            _logger.LogWarning("[Vip.Perk.Jumps] IVipShared or IVipPerkRegistry not available — perk inactive.");
            return;
        }
        _perk.SetDependencies(_bridge.VipShared!, _bridge.PerkRegistry!);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.Jumps] Registered.");
    }

    public void Shutdown() => _perk.Uninstall();
}
