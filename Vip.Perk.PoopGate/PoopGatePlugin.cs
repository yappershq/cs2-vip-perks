using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.PoopGate;

public sealed class PoopGatePlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.PoopGate";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<PoopGatePlugin> _logger;
    private readonly InterfaceBridge         _bridge;
    private readonly PoopGatePerk            _perk;

    public PoopGatePlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<PoopGatePlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new PoopGatePerk();
    }

    public bool Init() => true;

    public void PostInit() { }

    public void OnAllModulesLoaded()
    {
        if (!_bridge.Resolve())
        {
            _logger.LogWarning("[Vip.Perk.PoopGate] IVipShared, IVipPerkRegistry, or IPoopShared not available — perk inactive.");
            return;
        }

        _bridge.PoopShared!.CanUseFor += client => _bridge.VipShared!.IsVip(client.SteamId);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.PoopGate] Registered — poop restricted to VIPs.");
    }

    public void Shutdown() { }
}
