using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.ReservedSlotsGate;

public sealed class ReservedSlotsGatePlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.ReservedSlotsGate";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<ReservedSlotsGatePlugin> _logger;
    private readonly InterfaceBridge                  _bridge;
    private readonly ReservedSlotsGatePerk            _perk;

    public ReservedSlotsGatePlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ReservedSlotsGatePlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new ReservedSlotsGatePerk();
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
                "[Vip.Perk.ReservedSlotsGate] target plugin's Shared DLL is missing or outdated — perk inactive ({Ex}): {Msg}",
                ex.GetType().Name, ex.Message);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetupGate()
    {
        if (!_bridge.Resolve())
        {
            _logger.LogWarning("[Vip.Perk.ReservedSlotsGate] IVipShared, IVipPerkRegistry, or IReservedSlotsShared not available — perk inactive.");
            return;
        }

        _bridge.ReservedSlotsShared!.IsVipFor       += client  => _bridge.VipShared!.IsVip(client.SteamId);
        _bridge.ReservedSlotsShared!.IsVipForSteamId += steamId => _bridge.VipShared!.IsVip(steamId);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.ReservedSlotsGate] Registered — reserved slots restricted to VIPs.");
    }

    public void Shutdown() { }
}
