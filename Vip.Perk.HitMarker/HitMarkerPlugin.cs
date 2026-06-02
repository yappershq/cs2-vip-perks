using System;
using System.IO;
using System.Runtime.CompilerServices;
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
        try { SetupGate(); }
        catch (Exception ex) when (ex is FileNotFoundException or TypeLoadException
                                      or MissingMethodException or MissingMemberException)
        {
            _logger.LogWarning(
                "[Vip.Perk.HitMarker] target plugin's Shared DLL is missing or outdated — perk inactive ({Ex}): {Msg}",
                ex.GetType().Name, ex.Message);
        }
    }

    // Separated so the CLR's JIT-time assembly load for the gated plugin's
    // Shared DLL happens inside OnAllModulesLoaded's try block.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetupGate()
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
