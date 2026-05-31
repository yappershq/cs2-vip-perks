using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Vip.Perk.AntiFlash.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.AntiFlash;

internal sealed class AntiFlashPerk : IVipPerk, IEventListener
{
    public string Id          => "anti_flash";
    public string DisplayName => "Anti Flash";
    public string Description => "Reduces flashbang blindness for VIP players.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IEventManager      _eventManager;
    private readonly ILogger            _logger;
    private readonly AntiFlashConfig    _config;

    public AntiFlashPerk(ISharedSystem sharedSystem, ILogger logger, AntiFlashConfig config)
    {
        _eventManager = sharedSystem.GetEventManager();
        _logger       = logger;
        _config       = config;

        Settings =
        [
            new VipPerkSetting("mode", "Mode (1=team,2=self,3=both)", VipPerkSettingType.Int,
                _config.Mode.ToString(), "1", "3"),
        ];
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

        var attacker = e.GetPlayerController("attacker");
        if (attacker?.IsValidEntity is not true) return;

        var pawn = player.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return;

        prefs.Settings.TryGetValue("mode", out var raw);
        var mode = int.TryParse(raw, out var m) ? m : _config.Mode;

        var sameTeam = attacker.Team == player.Team;
        switch (mode)
        {
            case 1:
                if (sameTeam && player.SteamId != attacker.SteamId)
                    pawn.FlashDuration = 0f;
                break;
            case 2:
                if (player.SteamId == attacker.SteamId)
                    pawn.FlashDuration = 0f;
                break;
            case 3:
                if (sameTeam || player.SteamId == attacker.SteamId)
                    pawn.FlashDuration = 0f;
                break;
            default:
                pawn.FlashDuration = 0f;
                break;
        }
    }

    int IEventListener.ListenerVersion  => IEventListener.ApiVersion;
    int IEventListener.ListenerPriority => 0;
}
