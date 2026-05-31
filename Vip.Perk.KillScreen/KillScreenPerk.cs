using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.GameEvents;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Types.CppProtobuf;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.KillScreen;

internal sealed class KillScreenPerk : IVipPerk, IEventListener
{
    public string Id          => "kill_screen";
    public string DisplayName => "Kill Screen";
    public string Description => "Flashes a color on-screen when the VIP player gets a kill.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("duration", "Fade Duration (s)", VipPerkSettingType.Float, "2.0", "0.5", "10.0"),
        new VipPerkSetting("color",    "Color (R,G,B)",     VipPerkSettingType.String, "0,0,255"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IEventManager _eventManager;
    private readonly IModSharp     _modSharp;
    private readonly ILogger       _logger;

    public KillScreenPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _eventManager = sharedSystem.GetEventManager();
        _modSharp     = sharedSystem.GetModSharp();
        _logger       = logger;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()
    {
        _eventManager.HookEvent("player_death");
        _eventManager.InstallEventListener(this);
    }

    internal void Uninstall() => _eventManager.RemoveEventListener(this);

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    public void FireGameEvent(IGameEvent e)
    {
        if (!e.Name.Equals("player_death", StringComparison.OrdinalIgnoreCase)) return;
        if (e is not IEventPlayerDeath death) return;

        var killer = death.KillerController;
        var victim = death.VictimController;
        if (killer?.IsValidEntity is not true || victim?.IsValidEntity is not true) return;
        if (killer.SteamId == victim.SteamId) return;

        if (_vip?.IsVip(killer.SteamId) is not true) return;

        var prefs = _registry?.GetPreferences(killer.SteamId, Id);
        if (prefs is null || !prefs.Enabled) return;

        prefs.Settings.TryGetValue("duration", out var rawDur);
        prefs.Settings.TryGetValue("color",    out var rawColor);

        var duration = float.TryParse(rawDur, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 2.0f;

        byte r = 0, g = 0, b = 255;
        if (!string.IsNullOrWhiteSpace(rawColor))
        {
            var parts = rawColor.Split(',');
            if (parts.Length == 3)
            {
                byte.TryParse(parts[0].Trim(), out r);
                byte.TryParse(parts[1].Trim(), out g);
                byte.TryParse(parts[2].Trim(), out b);
            }
        }

        var filter = new RecipientFilter(killer.PlayerSlot);
        var fade = new CUserMessageFade
        {
            Duration = Convert.ToUInt32(duration * 512),
            HoldTime = Convert.ToUInt32(0.2f * 512),
            Flags    = 0x0001 | 0x0010,
            Color    = new Color32(r, g, b, 200),
        };
        _modSharp.SendNetMessage(filter, fade);
    }

    int IEventListener.ListenerVersion  => IEventListener.ApiVersion;
    int IEventListener.ListenerPriority => 0;
}
