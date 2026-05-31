using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Vip.Shared;
using Vip.Shared.Perks;

namespace Vip.Perk.Items;

internal sealed class ItemsPerk : IVipPerk
{
    public string Id          => "items";
    public string DisplayName => "Items";
    public string Description => "Grants extra weapons or grenades to VIP players on spawn.";
    public bool   DefaultEnabled => false;

    public IReadOnlyList<VipPerkSetting> Settings { get; } =
    [
        new VipPerkSetting("items", "Item class names (CSV, e.g. weapon_ak47)", VipPerkSettingType.String, ""),
    ];

    private IVipShared?       _vip;
    private IVipPerkRegistry? _registry;

    private readonly IHookManager _hookManager;
    private readonly IModSharp    _modSharp;
    private readonly ILogger      _logger;

    private readonly Action<IPlayerSpawnForwardParams> _onSpawn;

    public ItemsPerk(ISharedSystem sharedSystem, ILogger logger)
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

        prefs.Settings.TryGetValue("items", out var rawItems);
        if (string.IsNullOrWhiteSpace(rawItems)) return;

        var items = rawItems.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (items.Length == 0) return;

        _modSharp.InvokeFrameAction(() =>
        {
            var ctrl = controller;
            if (ctrl?.IsValidEntity is not true) return;
            var pawn = ctrl.GetPlayerPawn();
            if (pawn?.IsValidEntity is not true) return;

            foreach (var item in items)
            {
                if (!item.StartsWith("weapon_") && !item.StartsWith("item_")) continue;
                pawn.GiveNamedItem(item);
                _logger.LogInformation("[Vip.Perk.Items] gave {i} to {n}", item, ctrl.PlayerName);
            }
        });
    }
}
