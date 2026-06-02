using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Healthshot;

internal sealed class HealthshotPerk : IVipPerk
{
    public string Id             => "healthshot";
    public string DisplayName    => "Healthshot";
    public string Description    => "Grants a weapon_healthshot to VIPs each round.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("count", "How many healthshots to grant per spawn", VipPerkSettingType.Int, "1"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager _hookManager;
    private readonly IModSharp    _modSharp;
    private readonly ILogger      _logger;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public HealthshotPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _hookManager = sharedSystem.GetHookManager();
        _modSharp    = sharedSystem.GetModSharp();
        _logger      = logger;
        _onSpawn     = OnPlayerSpawn;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()   => _hookManager.PlayerSpawnPost.InstallForward(_onSpawn);
    internal void Uninstall() => _hookManager.PlayerSpawnPost.RemoveForward(_onSpawn);

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    private void OnPlayerSpawn(IPlayerSpawnForwardParams parms)
    {
        var controller = parms.Controller;
        if (controller?.IsValidEntity is not true) return;
        if (_vip?.IsVip(controller.SteamId) is not true) return;

        var prefs = _registry?.GetPreferences(controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return;

        var count = 1;
        if (prefs.Settings.TryGetValue("count", out var raw) && int.TryParse(raw, out var parsed) && parsed > 0)
            count = parsed;

        _modSharp.InvokeFrameAction(() =>
        {
            if (controller.IsValidEntity is not true) return;
            var pawn = controller.GetPlayerPawn();
            if (pawn?.IsValidEntity is not true) return;

            for (var i = 0; i < count; i++)
                pawn.GiveNamedItem("weapon_healthshot");

            _logger.LogDebug("[Vip.Perk.Healthshot] gave {Count} healthshot(s) to {Name}", count, controller.PlayerName);
        });
    }
}
