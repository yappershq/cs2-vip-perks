using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.GameEvents;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Vip.Perk.LifeSteal.Configuration;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.LifeSteal;

internal sealed class LifeStealPerk : IVipPerk, IEventListener
{
    public string Id          => "life_steal";
    public string DisplayName => "Life Steal";
    public string Description => "Restores a percentage of damage dealt as HP on kill.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; }

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IEventManager   _eventManager;
    private readonly ILogger         _logger;
    private readonly LifeStealConfig _config;

    public LifeStealPerk(ISharedSystem sharedSystem, ILogger logger, LifeStealConfig config)
    {
        _eventManager = sharedSystem.GetEventManager();
        _logger       = logger;
        _config       = config;

        Settings =
        [
            new VipPerkSetting("percent", "Steal % (0–100)", VipPerkSettingType.Float,
                _config.Percent.ToString(System.Globalization.CultureInfo.InvariantCulture), "0", "100"),
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

        var pawn = killer.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return;

        prefs.Settings.TryGetValue("percent", out var raw);
        var pct = float.TryParse(raw, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : _config.Percent;

        var heal  = (int)MathF.Floor(death.Damage * pct / 100.0f);
        var newHp = Math.Min(pawn.Health + heal, pawn.MaxHealth);
        pawn.Health = newHp;

        _logger.LogInformation("[Vip.Perk.LifeSteal] +{h}hp for {n}", heal, killer.PlayerName);
    }

    int IEventListener.ListenerVersion  => IEventListener.ApiVersion;
    int IEventListener.ListenerPriority => 0;
}
