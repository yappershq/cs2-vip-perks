using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Perk.Gravity.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Gravity;

internal sealed class GravityPerk : IVipPerk
{
    public string Id          => "gravity";
    public string DisplayName => "Gravity";
    public string Description => "Reduces gravity scale for VIP players.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager  _hookManager;
    private readonly ILogger       _logger;
    private readonly GravityConfig _config;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public GravityPerk(ISharedSystem sharedSystem, ILogger logger, GravityConfig config)
    {
        _hookManager = sharedSystem.GetHookManager();
        _logger      = logger;
        _config      = config;
        _onSpawn     = OnPlayerSpawn;

        Settings =
        [
            new VipPerkSetting("gravityScale", "Gravity Scale", VipPerkSettingType.Float,
                _config.GravityScale.ToString(System.Globalization.CultureInfo.InvariantCulture), "0.1", "1.0"),
        ];
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

        var pawn = controller.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return;

        prefs.Settings.TryGetValue("gravityScale", out var raw);
        var scale = float.TryParse(raw, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : _config.GravityScale;

        pawn.SetGravityScale(scale);
        _logger.LogInformation("[Vip.Perk.Gravity] scale={s} for {n}", scale, controller.PlayerName);
    }
}
