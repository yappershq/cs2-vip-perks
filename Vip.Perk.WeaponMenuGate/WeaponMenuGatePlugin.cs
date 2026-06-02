using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sharp.Shared;

namespace Vip.Perk.WeaponMenuGate;

public sealed class WeaponMenuGatePlugin : IModSharpModule
{
    public string DisplayName   => "Vip.Perk.WeaponMenuGate";
    public string DisplayAuthor => "yappershq";

    private readonly ILogger<WeaponMenuGatePlugin> _logger;
    private readonly InterfaceBridge               _bridge;
    private readonly WeaponMenuGatePerk            _perk;

    public WeaponMenuGatePlugin(ISharedSystem sharedSystem, string dllPath, string sharpPath,
        System.Version version, Microsoft.Extensions.Configuration.IConfiguration configuration, bool hotReload)
    {
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<WeaponMenuGatePlugin>();
        _bridge = new InterfaceBridge(sharedSystem);
        _perk   = new WeaponMenuGatePerk();
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
                "[Vip.Perk.WeaponMenuGate] target plugin's Shared DLL is missing or outdated — perk inactive ({Ex}): {Msg}",
                ex.GetType().Name, ex.Message);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetupGate()
    {
        if (!_bridge.Resolve())
        {
            _logger.LogWarning("[Vip.Perk.WeaponMenuGate] IVipShared, IVipPerkRegistry, or IWeaponMenuShared not available — perk inactive.");
            return;
        }

        _bridge.WeaponMenuShared!.CanUseFor += client => _bridge.VipShared!.IsVip(client.SteamId);
        _bridge.PerkRegistry!.Register(_perk);
        _logger.LogInformation("[Vip.Perk.WeaponMenuGate] Registered — weapon menu restricted to VIPs.");
    }

    public void Shutdown() { }
}
