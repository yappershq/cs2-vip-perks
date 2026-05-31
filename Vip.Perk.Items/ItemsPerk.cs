using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
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
        new VipPerkSetting("teamItems", "Team Items (JSON: {\"CT\":[...],\"T\":[...]})", VipPerkSettingType.String, ""),
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

        prefs.Settings.TryGetValue("teamItems", out var raw);
        if (string.IsNullOrWhiteSpace(raw)) return;

        var teamKey = controller.Team == CStrikeTeam.CT ? "CT" : "T";

        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(raw);
            if (!doc.RootElement.TryGetProperty(teamKey, out var arr)) return;

            _modSharp.InvokeFrameAction(() =>
            {
                var ctrl = controller;
                if (ctrl?.IsValidEntity is not true) return;
                var pawn = ctrl.GetPlayerPawn();
                if (pawn?.IsValidEntity is not true) return;

                foreach (var item in arr.EnumerateArray())
                {
                    var name = item.GetString();
                    if (name is null) continue;
                    if (!name.StartsWith("weapon_") && !name.StartsWith("item_")) continue;
                    pawn.GiveNamedItem(name);
                    _logger.LogInformation("[Vip.Perk.Items] gave {i} to {n}", name, ctrl.PlayerName);
                }
            });
        }
        catch { }
    }
}
