using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.AntiFlash;

internal sealed class AntiFlashPerk : IVipPerk, IEventListener
{
    public string Id          => "anti_flash";
    public string DisplayName => "Anti Flash";
    public string Description => "Reduces or removes flashbang blindness for VIP players.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("reductionPercent", "Reduction (0-1)", VipPerkSettingType.Float, "0.5", "0.0", "1.0"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IEventManager _eventManager;
    private readonly ILogger       _logger;

    public AntiFlashPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _eventManager = sharedSystem.GetEventManager();
        _logger       = logger;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()
    {
        _eventManager.HookEvent("player_blind");
        _eventManager.InstallEventListener(this);
    }

    internal void Uninstall() => _eventManager.RemoveEventListener(this);

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    public void FireGameEvent(IGameEvent e)
    {
        if (!e.Name.Equals("player_blind", StringComparison.OrdinalIgnoreCase)) return;

        var player = e.GetPlayerController("userid");
        if (player?.IsValidEntity is not true) return;
        if (_vip?.IsVip(player.SteamId) is not true) return;

        var prefs = _registry?.GetPreferences(player.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return;

        var pawn = player.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return;

        prefs.Settings.TryGetValue("reductionPercent", out var raw);
        var reduction = float.TryParse(raw, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : 0.5f;

        pawn.FlashDuration *= (1f - Math.Clamp(reduction, 0f, 1f));
    }

    int IEventListener.ListenerVersion  => IEventListener.ApiVersion;
    int IEventListener.ListenerPriority => 0;
}
