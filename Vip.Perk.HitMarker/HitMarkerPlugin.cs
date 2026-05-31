using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.HitMarker.Configuration;

namespace Vip.Perk.HitMarker;

public sealed class HitMarkerPlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.HitMarker";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<HitMarkerPlugin> _logger;
    private readonly InterfaceBridge          _bridge;
    private readonly HitMarkerConfig          _config;
    private readonly HitMarkerPerk            _perk;

    public HitMarkerPlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<HitMarkerPlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = HitMarkerConfig.Load(sharpPath);
        _perk   = new HitMarkerPerk();
    }

    public bool Init() => true;

    public void PostInit() { }

    public void OnAllModulesLoaded()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.HitMarker] Disabled via config — skipping.");
            return;
        }

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
