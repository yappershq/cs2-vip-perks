using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.HitMarker;

public sealed class HitMarkerPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.HitMarker";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<HitMarkerPlugin> _logger;
    private readonly InterfaceBridge          _bridge;
    private readonly HitMarkerPerk            _perk;

    public HitMarkerPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<HitMarkerPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new HitMarkerPerk();
    }

    public bool Init() => true;

    public void PostInit() { }

    public void OnAllModulesLoaded()
    {
        if (!_bridge.Resolve())
        {
            _logger.LogWarning("[Vip.Perk.HitMarker] IVipShared, IVipPerkRegistry, or IHitMarkShared not available — perk inactive.");
            return;
        }

        _bridge.HitMarkShared!.CanShowFor += client => _bridge.VipShared!.IsVip(client.SteamId);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.HitMarker] Registered — damage numbers restricted to VIPs.");
    }

    public void Shutdown() { }
}
