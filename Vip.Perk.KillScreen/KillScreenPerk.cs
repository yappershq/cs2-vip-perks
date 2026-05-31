using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEvents;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Types.CppProtobuf;
using Vip.Perk.KillScreen.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.KillScreen;

internal sealed class KillScreenPerk : IVipPerk, IEventListener
{
    public string Id          => "kill_screen";
    public string DisplayName => "Kill Screen";
    public string Description => "Flashes a color on-screen when the VIP player gets a kill.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IEventManager     _eventManager;
    private readonly IModSharp         _modSharp;
    private readonly ILogger           _logger;
    private readonly KillScreenConfig  _config;

    public KillScreenPerk(ISharedSystem sharedSystem, ILogger logger, KillScreenConfig config)
    {
        _eventManager = sharedSystem.GetEventManager();
        _modSharp     = sharedSystem.GetModSharp();
        _logger       = logger;
        _config       = config;

        _config.TeamFadeColor.TryGetValue("CT", out var ct);
        _config.TeamFadeColor.TryGetValue("T", out var t);
        var ctDefault = ct is { Length: 4 } ? $"{ct[0]},{ct[1]},{ct[2]},{ct[3]}" : "255,0,0,180";
        var tDefault  = t  is { Length: 4 } ? $"{t[0]},{t[1]},{t[2]},{t[3]}"   : "0,0,255,180";

        Settings =
        [
            new VipPerkSetting("fadeColorCT", "Fade Color CT (R,G,B,A)", VipPerkSettingType.String, ctDefault),
            new VipPerkSetting("fadeColorT",  "Fade Color T (R,G,B,A)",  VipPerkSettingType.String, tDefault),
            new VipPerkSetting("duration",    "Fade Duration (s)",         VipPerkSettingType.Float, "2.0", "0.5", "10.0"),
        ];
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

        var teamKey    = killer.Team == CStrikeTeam.CT ? "CT" : "T";
        var settingKey = killer.Team == CStrikeTeam.CT ? "fadeColorCT" : "fadeColorT";

        prefs.Settings.TryGetValue("duration", out var rawDur);
        prefs.Settings.TryGetValue(settingKey, out var rawColor);

        var duration = float.TryParse(rawDur, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 2.0f;

        byte r = 255, g = 0, b = 0, a = 180;
        if (!string.IsNullOrWhiteSpace(rawColor))
        {
            var parts = rawColor.Split(',');
            if (parts.Length == 4)
            {
                byte.TryParse(parts[0].Trim(), out r);
                byte.TryParse(parts[1].Trim(), out g);
                byte.TryParse(parts[2].Trim(), out b);
                byte.TryParse(parts[3].Trim(), out a);
            }
        }
        else if (_config.TeamFadeColor.TryGetValue(teamKey, out var cfg) && cfg.Length == 4)
        {
            r = (byte)cfg[0]; g = (byte)cfg[1]; b = (byte)cfg[2]; a = (byte)cfg[3];
        }

        var filter = new RecipientFilter(killer.PlayerSlot);
        var fade = new CUserMessageFade
        {
            Duration = Convert.ToUInt32(duration * 512),
            HoldTime = Convert.ToUInt32(0.2f * 512),
            Flags    = 0x0001 | 0x0010,
            Color    = new Color32(r, g, b, a),
        };
        _modSharp.SendNetMessage(filter, fade);
        _logger.LogInformation("[Vip.Perk.KillScreen] fade {r},{g},{b},{a} for {n}", r, g, b, a, killer.PlayerName);
    }

    int IEventListener.ListenerVersion  => IEventListener.ApiVersion;
    int IEventListener.ListenerPriority => 0;
}
