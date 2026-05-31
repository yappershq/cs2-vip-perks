using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Types;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Speed;

internal sealed class SpeedPerk : IVipPerk
{
    public string Id          => "speed";
    public string DisplayName => "Speed";
    public string Description => "Increases VIP player movement speed.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("multiplier", "Speed Multiplier", VipPerkSettingType.Float, "1.3", "1.0", "2.0"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager _hookManager;
    private readonly ILogger      _logger;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;
    private readonly Func<IPlayerGetMaxSpeedHookParams, HookReturnValue<float>, HookReturnValue<float>> _onGetMaxSpeed;

    public SpeedPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _hookManager   = sharedSystem.GetHookManager();
        _logger        = logger;
        _onSpawn       = OnPlayerSpawn;
        _onGetMaxSpeed = OnPlayerGetMaxSpeedPre;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()
    {
        _hookManager.PlayerSpawnPost.InstallForward(_onSpawn);
        _hookManager.PlayerGetMaxSpeed.InstallHookPre(_onGetMaxSpeed);
    }

    internal void Uninstall()
    {
        _hookManager.PlayerSpawnPost.RemoveForward(_onSpawn);
        _hookManager.PlayerGetMaxSpeed.RemoveHookPre(_onGetMaxSpeed);
    }

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

        prefs.Settings.TryGetValue("multiplier", out var raw);
        var mult = float.TryParse(raw, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : 1.3f;

        pawn.VelocityModifier = mult;
        _logger.LogInformation("[Vip.Perk.Speed] mult={m} for {n}", mult, controller.PlayerName);
    }

    private HookReturnValue<float> OnPlayerGetMaxSpeedPre(IPlayerGetMaxSpeedHookParams parms, HookReturnValue<float> original)
    {
        if (parms.Controller?.IsValidEntity is not true) return original;
        if (_vip?.IsVip(parms.Controller.SteamId) is not true) return original;

        var prefs = _registry?.GetPreferences(parms.Controller.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return original;

        prefs.Settings.TryGetValue("multiplier", out var raw);
        var mult = float.TryParse(raw, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : 1.3f;

        return new HookReturnValue<float>(EHookAction.SkipCallReturnOverride, mult * 250f);
    }
}
