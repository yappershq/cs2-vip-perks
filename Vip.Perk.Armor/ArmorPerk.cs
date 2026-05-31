using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Armor;

internal sealed class ArmorPerk : IVipPerk
{
    public string Id          => "armor";
    public string DisplayName => "Armor";
    public string Description => "Grants kevlar (and optionally helmet) on spawn.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("kevlar", "Kevlar Amount", VipPerkSettingType.Int,  "100", "1", "100"),
        new VipPerkSetting("helmet", "Give Helmet",   VipPerkSettingType.Bool, "true"),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager _hookManager;
    private readonly ILogger      _logger;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public ArmorPerk(ISharedSystem sharedSystem, ILogger logger)
    {
        _hookManager = sharedSystem.GetHookManager();
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

        var pawn = controller.GetPlayerPawn();
        if (pawn?.IsValidEntity is not true) return;

        prefs.Settings.TryGetValue("kevlar", out var rawKevlar);
        prefs.Settings.TryGetValue("helmet", out var rawHelmet);

        var kevlar = int.TryParse(rawKevlar, out var k)   ? k    : 100;
        var helmet = !bool.TryParse(rawHelmet, out var h) || h;

        pawn.ArmorValue = kevlar;
        var itemSvc = pawn.GetItemService();
        if (itemSvc is not null) itemSvc.HasHelmet = helmet;

        _logger.LogInformation("[Vip.Perk.Armor] kevlar={k} helmet={h} for {n}", kevlar, helmet, controller.PlayerName);
    }
}
