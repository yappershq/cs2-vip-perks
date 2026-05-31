using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Types;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.SmokeColor;

internal sealed class SmokeColorPerk : IVipPerk, IEntityListener
{
    public string Id          => "smoke_color";
    public string DisplayName => "Smoke Color";
    public string Description => "Changes the smoke grenade color for VIP players based on team.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("teamColor", "Team Color (CT/T JSON)", VipPerkSettingType.String, "CT:0,180,255 T:255,180,0"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IEntityManager _entityManager;
    private readonly IModSharp      _modSharp;
    private readonly ILogger        _logger;

    public SmokeColorPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _entityManager = sharedSystem.GetEntityManager();
        _modSharp      = sharedSystem.GetModSharp();
        _logger        = logger;
    }

    internal void SetDependencies(IVipShared vip, IVipPerkRegistry registry)
    {
        _vip      = vip;
        _registry = registry;
    }

    internal void Install()   => _entityManager.InstallEntityListener(this);
    internal void Uninstall() => _entityManager.RemoveEntityListener(this);

    public void OnPreferencesChanged(ulong steamId, VipPerkPreferences prefs) { }

    public void OnEntityCreated(IBaseEntity ent)
    {
        if (ent?.IsValidEntity is not true) return;

        _modSharp.InvokeFrameAction(() =>
        {
            if (ent?.IsValidEntity is not true) return;
            if (ent.Classname is not "smokegrenade_projectile") return;
            if (ent.OwnerEntity?.IsValidEntity is not true) return;

            var throwerPawn = _entityManager.FindEntityByIndex<IPlayerPawn>(ent.OwnerEntity.Index);
            if (throwerPawn?.IsValidEntity is not true) return;

            var controller = throwerPawn.GetOriginalController();
            if (controller?.IsValidEntity is not true) return;

            if (_vip?.IsVip(controller.SteamId) is not true) return;

            var prefs = _registry?.GetPreferences(controller.SteamId, Id);
            if (prefs is null || !prefs.Enabled) return;

            var teamKey = controller.Team == CStrikeTeam.CT ? "CT" : "T";
            int r = 255, g = 0, b = 255;

            ent.SetNetVar("m_vSmokeColor", new Vector(r, g, b));
            _logger.LogInformation("[Vip.Perk.SmokeColor] {r},{g},{b} for {n}", r, g, b, controller.PlayerName);
        });
    }

    int IEntityListener.ListenerVersion  => IEntityListener.ApiVersion;
    int IEntityListener.ListenerPriority => 0;
}
