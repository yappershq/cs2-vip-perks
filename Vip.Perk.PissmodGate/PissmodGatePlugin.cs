using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Vip.Perk.PissmodGate.Configuration;

namespace Vip.Perk.PissmodGate;

public sealed class PissmodGatePlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.PissmodGate";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<PissmodGatePlugin> _logger;
    private readonly InterfaceBridge            _bridge;
    private readonly PissmodGateConfig          _config;
    private readonly PissmodGatePerk            _perk;

    public PissmodGatePlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<PissmodGatePlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _config = PissmodGateConfig.Load(sharpPath);
        _perk   = new PissmodGatePerk();
    }

    public bool Init() => true;

    public void PostInit() { }

    public void OnAllModulesLoaded()
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("[Vip.Perk.PissmodGate] Disabled via config — skipping.");
            return;
        }

        if (!_bridge.Resolve())
        {
            _logger.LogWarning("[Vip.Perk.PissmodGate] IVipShared, IVipPerkRegistry, or IPissModShared not available — perk inactive.");
            return;
        }

        _bridge.PissModShared!.CanUseFor += client => _bridge.VipShared!.IsVip(client.SteamId);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.PissmodGate] Registered — pissmod restricted to VIPs.");
    }

    public void Shutdown() { }
}
